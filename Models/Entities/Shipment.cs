using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRoute.Models.Entities
{
    public class Shipment
    {
        [Key]
        public int Id{get; set;}

        public string ShipmentCode{get; set;} = string.Empty;

        public double ShipmentCO2Emission{get; set;}

        public DateTime ShipmentDate{get; set;} = DateTime.Now;

        public int ShipmentTotalItems{get; set;}

        public double ShipmentWeightKg{get; set;}

        public double ShipmentLength{get; set;}

        public double ShipmentWidth{get; set;}

        public double ShipmentHeight{get; set;}

        public string ShipmentOrgin{get; set;} = string.Empty;

        public string ShipmentDestination{get; set;} = string.Empty;

        public double ShipmentDistance{get; set;}

        public string ShipmentMode{get; set;}

        public string Vehicle{get; set;} = string.Empty;
        public List<Order> OrderList{get; set;} = new List<Order>();

        public int TransportCompanyId{get; set;}
    }
}