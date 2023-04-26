namespace PowerPlantAPI.Models
{
    public class PowerPlantDto
    {
        public string Name { get; set; }

        public PlantType Type { get; set; }

        public decimal Efficiency { get; set; }

        public int PMin { get; set; }

        public int PMax { get; set; }
    }
}
