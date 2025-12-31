using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRoute.Models.Entities
{
    public class Order
    {

        [Key]
        public int Id{get; set;}

        public string OrderCode{get; set;} = string.Empty;

        public double OrderCO2Emission{get; set;}

        public string OrderOrigin{get; set;} = string.Empty;

        public string OrderDestination{get; set;} = string.Empty;

        public string SelectedRouteSummary{get; set;} = string.Empty;

        public string SelectedPolyline{get; set;} = string.Empty;

        public double OrderDistance{get; set;}

        public double OrderWeightKg{get; set;}

        public double OrderLength{get; set;}

        public double OrderWidth{get; set;}

        public double OrderHeight{get; set;}

        public int OrderTotalItems{get; set;}

        
        public RoutePoint OriginRP{get; set;}
        
        public RoutePoint DestinationRP{get; set;}
        
        public string? OrderStatus{get; set;}

        public DateTime OrderDate{get; set;} = DateTime.Now;

        public string OrderNature {get; set;} = string.Empty; // UPSTREAM OR DOWNSTREAM

        public string TransportMode {get; set;} = "Ground"; // WE DO ONLY FOR GROUND TRANS

        public string? TransportVehicle{get; set;}
        public string OrderMode {get; set;} = string.Empty; // SHARED OR DEDICATED
        
        public bool IsRefrigerated{get; set;}

        public double OrderStandardCO2Emissions{get; set;}

        public double OrderEmissionsSaved => OrderStandardCO2Emissions - OrderCO2Emission;

        public int TransportCompanyId{get; set;}

        public string TransportCompanyName{get; set;} = string.Empty;

        public bool? IsAutoApproved{get; set;}

        //relationships
        public string CompanyName{get; set;}

        public int CompanyId{get; set;}
        [ForeignKey("CompanyId")]
        public Company Company{get; set;}

        public int? ShipmentId{get; set;}
        [ForeignKey("ShipmentId")]
        public Shipment Shipment{get; set;}

        public bool IsRender{get; set;}
    }
}