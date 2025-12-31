using EcoRoute.Models.HelperClasses;

namespace EcoRoute.Models.DTOs
{
    public class ClusterPackingInputDto
    {
        public double TotalWeightKg { get; set; }
        public List<PackableItem> Items { get; set; }
    }
}