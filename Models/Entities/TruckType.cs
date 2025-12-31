namespace EcoRoute.Models.Entities
{
    public class TruckType
    {
        public int Id{get; set;}

        public string TruckName{get; set;} = string.Empty;

        public double GVWKg {get; set;}

        public double MaxPayloadKg{get;set;}

        public double CargoLengthMeters{get; set;}
        public double CargoWidthMeters{get; set;}
        public double CargoHeightMeters{get; set;}

        public double KerbWeight{get; set;}
        public double EngineEfficiency{get; set;} = 0.40;
        public double DragCoefficient{get; set;} = 0.6;

        public bool IsRefrigerated{get; set;}

    }
}