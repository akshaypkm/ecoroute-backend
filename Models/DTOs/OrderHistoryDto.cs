namespace EcoRoute.Models
{
    public class OrderHistoryDto
    {
        public string ShipmentCode{get; set;} = "-";

        public DateTime OrderDate{get; set;}

        public string OrderOrigin{get; set;}

        public string OrderDestination{get; set;}

        public int OrderTotalItems{get; set;}

        public double OrderWeightKg{get; set;}

        public double OrderCO2Emission{get; set;}

        public double OrderEmissionsSaved{get; set;}

        public string OrderStatus{get; set;}
    }
}