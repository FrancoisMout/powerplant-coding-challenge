using System.Text.Json.Serialization;

namespace PowerPlantAPI.Models
{
    public class EnergySources
    {
        [JsonPropertyName("gas(euro/MWh)")]
        public decimal GasCost { get; set; }

        [JsonPropertyName("kerosine(euro/MWh)")]
        public decimal KerosineCost { get; set; }

        [JsonPropertyName("co2(euro/ton)")]
        public decimal CarbonCost { get; set; }

        [JsonPropertyName("wind(%)")]
        public decimal WindPercent { get; set; }
    }
}
