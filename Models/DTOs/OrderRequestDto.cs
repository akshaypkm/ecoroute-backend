namespace EcoRoute.Models
{
    public class OrderRequestDto
    {
        public string OrderNature{get; set;} //UP OR DOWN
        public string TransportMode{get; set;} // GROUND / AIR / SEA

        public int OrderTotalItems{get; set;}
        public double OrderWeightKg{get; set;}

        //DIMENSIONS FOR ONE ITEM
        
        public double OrderLength{get; set;}

        public double OrderWidth{get; set;}

        public double OrderHeight{get; set;}

        public string OrderMode{get; set;} //SHARED OR DEDI

        public bool IsRefrigerated{get; set;}

        public string OrderOrigin{get; set;}

        public string OrderDestination{get; set;}

        public DateTime OrderDate{get; set;}

        public string TransportCompanyName{get; set;} = string.Empty;
    }
}