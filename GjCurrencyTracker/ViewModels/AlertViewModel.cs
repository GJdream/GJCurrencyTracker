using System.Collections.ObjectModel;
using System.Windows.Input;
using GjCurrencyTracker.Models;
using GjCurrencyTracker.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GjCurrencyTracker.Services;
using Microsoft.Maui.Animations;

namespace GjCurrencyTracker.ViewModels
{
    public class AlertViewModel : INotifyPropertyChanged
    {
        AlertRule SelectedAlert = null;
        public ICommand EditAlertCommand { get; }
        public ICommand DeleteAlertCommand { get; }
        private readonly ExchangeRateService _exchangeRateService;
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

        private double _targetValue;
        public double TargetValue
        {
            get => _targetValue;
            set { _targetValue = value; OnPropertyChanged(); }
        }

        private string _selectedBaseCurrency;
        public string SelectedBaseCurrency
        {
            get => _selectedBaseCurrency;
            set
            {
                _selectedBaseCurrency = value;
                OnPropertyChanged();
            }
        }

        private double _currentValue;
        public double CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<AlertRule> ExistingAlerts { get; set; } = new();

        public ICommand SaveAlertCommand { get; }

        public AlertViewModel()
        {
            EditAlertCommand = new Command<AlertRule>(EditAlert);
            DeleteAlertCommand = new Command<AlertRule>(DeleteAlert);
            _exchangeRateService = new ExchangeRateService();
            SaveAlertCommand = new Command(async () => await SaveAlertAsync());
            LoadAlerts();
        }


        public async void OnCurrencySelectionChanged()
        {
            if (!string.IsNullOrEmpty(SelectedBaseCurrency) && !string.IsNullOrEmpty(SelectedCurrency))
            {
                var currentRate = await _exchangeRateService.GetExchangeRateAsync(SelectedBaseCurrency, SelectedCurrency);
                CurrentValue = currentRate; // Bind this to the UI if needed
            }
        }

        private async Task SaveAlertAsync()
        {
            if (SelectedAlert == null)
            {
                if (TargetValue > 0 && !string.IsNullOrWhiteSpace(SelectedCurrency) && !string.IsNullOrWhiteSpace(SelectedCondition))
                {
                    var alert = new AlertRule
                    {
                        CurrentValue = CurrentValue,
                        BaseCurrency = SelectedBaseCurrency,
                        TargetCurrency = SelectedCurrency,
                        Condition = SelectedCondition,
                        TargetValue = TargetValue
                    };

                   int result =  await App.Database.SaveAlertRuleAsync(alert);
                    if(result > 0)
                    {
                        SelectedBaseCurrency = "";
                        SelectedCurrency = "";
                        SelectedCondition = "";
                        TargetValue = 0;
                        CurrentValue = 0;
                    }
                    ExistingAlerts.Add(alert);
                }
            }
            else
            {
                    SelectedAlert.CurrentValue = CurrentValue;
                    SelectedAlert.BaseCurrency = SelectedBaseCurrency;
                    SelectedAlert.TargetCurrency = SelectedCurrency;
                    SelectedAlert.Condition = SelectedCondition;
                    SelectedAlert.TargetValue = TargetValue;
                await App.Database.SaveAlertRuleAsync(SelectedAlert);

                var index = ExistingAlerts.IndexOf(SelectedAlert);
                if (index >= 0)
                {
                    ExistingAlerts[index] = SelectedAlert;
                }
            }
        }

        private async void LoadAlerts()
        {
            var alerts = await App.Database.GetAlertRulesAsync();
            ExistingAlerts.Clear();
            foreach (var alert in alerts)
                ExistingAlerts.Add(alert);
        }

        private async void EditAlert(AlertRule alert)
        {
            // Populate your fields with the selected alert
            SelectedAlert = alert;
            SelectedBaseCurrency = alert.BaseCurrency;
            SelectedCurrency = alert.TargetCurrency;
            SelectedCondition = alert.Condition;
            TargetValue = alert.TargetValue;
            // Maybe open an "Edit" form or just fill existing inputs
        }

        private async void DeleteAlert(AlertRule alert)
        {
            await App.Database.DeleteAlertRuleAsync(alert);
             LoadAlerts(); // Refresh list
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}