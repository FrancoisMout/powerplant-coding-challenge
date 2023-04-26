namespace PowerPlantAPI.Models
{
    public class PowerPlant
    {
        public string Name { get; set; }

        public PlantType Type { get; set; }

        public decimal PMin { get; set; }

        public decimal PMax { get; set; }

        public decimal CostPerMWh { get; set; }

        public decimal Load { get; set; }
    }
}
