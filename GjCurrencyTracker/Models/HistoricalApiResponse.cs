// Placeholder for AlertRule.cs
using Newtonsoft.Json;
using SQLite;

namespace GjCurrencyTracker.Models
{
    public class HistoricalApiResponse
    {
        [JsonProperty("rates")]
        public Dictionary<string, Dictionary<string, double>> Rates { get; set; }
    }
}
