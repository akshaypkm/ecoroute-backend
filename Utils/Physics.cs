namespace EcoRoute.Services
{
    public class Physics
    {

        public const double G = 9.80665;
        public const double LHV_DIESEL_J_PER_L = 36e6;
        public const double CO2_PER_L = 2.7;

        public const double Crr = 0.01;

        
        internal static double EffectiveMassPerTonne(double kerbWeight, double orderWeightKg)
        {
            return 1000.0 * (1.0 + kerbWeight / orderWeightKg);
        }


        internal static (double TotalKgCO2PerTruckKgPerTonne, 
                        double totalJ_per_tonne, double totalLiters_per_tonne,
                        double totalKgCO2_per_tonne) ComputeGradeCO2AndExport
                        (
                            List<DensePoint> densePoints, double massPerTonne,
                            double ENGINE_EFF,
                            double elevationNoiseThresholdMeters = 0.5
                        )
        {
            for(int i = 0; i< densePoints.Count - 1; i++)
            {
                if(double.IsNaN(densePoints[i].Elevation) || double.IsNaN(densePoints[i + 1].Elevation))
                {
                    throw new ArgumentException("All dense points must be filled.");
                }

                densePoints[i].DeltaH = densePoints[i+1].Elevation - densePoints[i].Elevation;
                densePoints[i].SegmentMeters = Geometry.HaversineMeters(densePoints[i].Lat, densePoints[i].Lng, densePoints[i+1].Lat, densePoints[i+1].Lng);
            }
            // FOR DEBUGS
            double totalPositiveGain_all = 0.0;
            double totalPositiveGain_included = 0.0;
            for (int x = 0; x < densePoints.Count - 1; x++)
            {
                double dh = densePoints[x].DeltaH;
                if (dh > 0) totalPositiveGain_all += dh;
                if (dh > elevationNoiseThresholdMeters) totalPositiveGain_included += dh;
            }
            Console.WriteLine($"[DEBUG] totalPositiveGain_all (dh>0): {totalPositiveGain_all:F3} m");
            Console.WriteLine($"[DEBUG] totalPositiveGain_included (dh>{elevationNoiseThresholdMeters}): {totalPositiveGain_included:F3} m");
            
            int countAll = densePoints.Count(p => p.DeltaH > 0);
            int countIncluded = densePoints.Count( p=> p.DeltaH > elevationNoiseThresholdMeters);
            Console.WriteLine($"[DEBUG] uphill segments count: all={countAll}, included={countIncluded}");

            if(densePoints.Count > 0)
            {
                densePoints[densePoints.Count - 1].DeltaH = 0.0; 
                densePoints[densePoints.Count - 1].SegmentMeters = 0.0; 
                //WHY IS THIS PERFORMED ?
            }

            double totalJ_per_tonne = 0.0;
            double cumDist = 0.0;

            for(int i = 0; i< densePoints.Count - 1; i++)
            {
                var cur = densePoints[i];
                double segM = cur.SegmentMeters;
                double dh = cur.DeltaH;
                double upHillJ = 0.0;
                double baseJ = 0.0;

                if(dh > elevationNoiseThresholdMeters)
                {
                    upHillJ = massPerTonne * G * dh;
                }
                
                baseJ = massPerTonne * G * segM * Crr;

                double totalSegmentJ = baseJ + upHillJ;

                double liters = totalSegmentJ / (LHV_DIESEL_J_PER_L * ENGINE_EFF);

                double kgCO2 = liters * CO2_PER_L;

                totalJ_per_tonne += totalSegmentJ;

                cumDist += segM;
            }

            double totalLiters_per_tonne = totalJ_per_tonne / (LHV_DIESEL_J_PER_L * ENGINE_EFF);
            double totalKgCO2_per_tonne = totalLiters_per_tonne * CO2_PER_L;

            // Also compute scaled per-truck (massPerTonne factor included already in J calc if massPerTonne != 1000)
            // For clarity, return both per-tonne and also "per-truck contribution per tonne basis"
            double totalKgCO2_per_truck_kg_per_tonne = totalKgCO2_per_tonne; // same numeric; caller knows massPerTonne used

            // WHAT IS THE ABOVE THING

            return (totalKgCO2_per_truck_kg_per_tonne, totalJ_per_tonne
                , totalLiters_per_tonne, totalKgCO2_per_tonne);
        }
    }
}


