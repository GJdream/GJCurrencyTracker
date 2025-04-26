// Placeholder for AlertRule.cs
using SQLite;

namespace GjCurrencyTracker.Models
{
    public class AlertRule
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string TargetCurrency { get; set; }  // e.g., EUR, INR
        public double TargetValue { get; set; }     // e.g., 1.1 or 2.0
        public string Condition { get; set; }       // e.g., ">", "<", "Drop"
        public bool IsTriggered { get; set; }       // Whether the alert has been triggered
    }
}
