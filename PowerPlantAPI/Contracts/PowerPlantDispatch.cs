using System.Text.Json.Serialization;

namespace PowerPlantAPI.Contracts
{
    public class PowerPlantDispatch
    {
        public string Name { get; set; }

        [JsonPropertyName("p")]
        public decimal Load { get; set; }
    }
}
