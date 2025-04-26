using System.Collections.Generic;
using System.Linq;
using GjCurrencyTracker.Models;

namespace GjCurrencyTracker.Services
{
    public class AlertService
    {
        public void NotifyAlert(AlertRule alert)
        {
            string message = $" Alert: {alert.TargetCurrency} {alert.Condition} {alert.TargetValue}";

            // In a real app: Show a local notification, push message, or dialog


            // Optionally show popup (if UI is available)
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("Alert Triggered", message, "OK");
            });
        }
    }
}