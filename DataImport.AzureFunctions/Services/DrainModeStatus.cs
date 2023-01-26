using System.Text.Json.Serialization;


namespace DataImport.AzureFunctions.Services
{
    public class DrainModeStatus
    {
        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DrainModeState State { get; set; }

        [JsonPropertyName("outstandingInvocations")]
        public int OutstandingInvocations { get; set; }

        [JsonPropertyName("outstandingRetries")]
        public int OutstandingRetries { get; set; }
    }
}
