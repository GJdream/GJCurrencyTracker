using System.Collections.ObjectModel;
using System.Windows.Input;
using GjCurrencyTracker.Models;
using GjCurrencyTracker.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GjCurrencyTracker.ViewModels
{
    public class AlertViewModel : BindableObject
    {
        public ObservableCollection<string> AvailableCurrencies { get; set; } = new() { "EUR", "INR", "JPY", "USD" };
        public ObservableCollection<string> Conditions { get; set; } = new() { ">", "<", "% drop" };

        private string _selectedCurrency;
        public string SelectedCurrency
        {
            get => _selectedCurrency;
            set { _selectedCurrency = value; OnPropertyChanged(); }
        }

        private string _selectedCondition;
        public string SelectedCondition
        {
            get => _selectedCondition;
            set { _selectedCondition = value; OnPropertyChanged(); }
        }

        private string _targetValue;
        public string TargetValue
        {
            get => _targetValue;
            set { _targetValue = value; OnPropertyChanged(); }
        }

        public ObservableCollection<AlertRule> ExistingAlerts { get; set; } = new();

        public ICommand SaveAlertCommand { get; }

        public AlertViewModel()
        {
            SaveAlertCommand = new Command(async () => await SaveAlertAsync());
            LoadAlerts();
        }

        private async Task SaveAlertAsync()
        {
            if (double.TryParse(TargetValue, out double value) && !string.IsNullOrWhiteSpace(SelectedCurrency) && !string.IsNullOrWhiteSpace(SelectedCondition))
            {
                var alert = new AlertRule
                {
                    TargetCurrency = SelectedCurrency,
                    Condition = SelectedCondition,
                    TargetValue = value
                };

                await App.Database.SaveAlertRuleAsync(alert);
                ExistingAlerts.Add(alert);
            }
        }

        private async void LoadAlerts()
        {
            var alerts = await App.Database.GetAlertRulesAsync();
            ExistingAlerts.Clear();
            foreach (var alert in alerts)
                ExistingAlerts.Add(alert);
        }
    }
}