using PowerPlantAPI.Models;

namespace PowerPlantAPI.Contracts
{
    public class ProductionPlanRequest
    {
        public decimal Load { get; set; }

        public EnergySources Fuels { get; set; }

        public List<PowerPlantDto> PowerPlants { get; set; }
    }
}
