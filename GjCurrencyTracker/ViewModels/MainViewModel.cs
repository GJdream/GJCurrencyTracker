using GjCurrencyTracker.Data;
using GjCurrencyTracker.Models;
using GjCurrencyTracker.Services;
using GjCurrencyTracker.Views;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using Timer = System.Timers.Timer;

namespace GjCurrencyTracker.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ExchangeRateService _exchangeRateService;
        private readonly AlertService _alertService;
        private Timer _timer;
        public ICommand RefreshCommand { get; }
        public ICommand ToggleCurrencyCommand { get; }
        private List<AlertRule> _alertRules;
        private Dictionary<string, double> _previousRates = new Dictionary<string, double>(); // Store previous rates

        public ObservableCollection<ExchangeRate> Rates { get; set; } = new ObservableCollection<ExchangeRate>();
        public ObservableCollection<string> AvailableCurrencies { get; set; } = new ObservableCollection<string> { "USD", "EUR", "INR", "JPY" };

        private string _selectedBaseCurrency;
        public string SelectedBaseCurrency
        {
            get => _selectedBaseCurrency;
            set
            {
                if (_selectedBaseCurrency != value)
                {
                    _selectedBaseCurrency = value;
                    OnPropertyChanged();
                    FetchExchangeRates();
                }
            }
        }

        public MainViewModel()
        {
            _exchangeRateService = new ExchangeRateService();
            _alertService = new AlertService();
            RefreshCommand = new Command(ExecuteRefresh);
            ToggleCurrencyCommand = new Command<string>(OnCurrencyToggle);
            SelectedBaseCurrency = AvailableCurrencies.First();

            _selectedBaseCurrency = Preferences.Get(nameof(SelectedBaseCurrency), "USD");

            // Load previously selected Target Currencies
            var savedTargets = Preferences.Get("SelectedTargetCurrencies", "");
            if (!string.IsNullOrEmpty(savedTargets))
            {
                _selectedTargetCurrencies = new ObservableCollection<string>(savedTargets.Split(','));
            }

            LoadAlertRulesAsync(); // Load alert rules from local database
            StartTimer();
            FetchExchangeRates();
        }

        private void OnCurrencyToggle(string currency)
        {
            if (_selectedTargetCurrencies.Contains(currency))
            {
                _selectedTargetCurrencies.Remove(currency);  // Remove from list if unchecked
            }
            else
            {
                _selectedTargetCurrencies.Add(currency);  // Add to list if checked
            }
            FetchExchangeRates();  // Refresh the exchange rates based on updated target currencies
        }

        private void ExecuteRefresh()
        {
            FetchExchangeRates(); // Manually refresh when the button is clicked
        }

        private ObservableCollection<string> _selectedTargetCurrencies = new ObservableCollection<string>();
        public ObservableCollection<string> SelectedTargetCurrencies
        {
            get => _selectedTargetCurrencies;
            set
            {
                if (_selectedTargetCurrencies != value)
                {
                    if (_selectedTargetCurrencies != null)
                        _selectedTargetCurrencies.CollectionChanged -= SelectedTargetCurrencies_CollectionChanged;

                    _selectedTargetCurrencies = value;

                    if (_selectedTargetCurrencies != null)
                        _selectedTargetCurrencies.CollectionChanged += SelectedTargetCurrencies_CollectionChanged;

                    SaveSelectedTargetCurrencies();
                    OnPropertyChanged();
                }
            }
        }

        private void SelectedTargetCurrencies_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SaveSelectedTargetCurrencies();
        }

        private void SaveSelectedTargetCurrencies()
        {
            if (SelectedTargetCurrencies != null)
            {
                Preferences.Set("SelectedTargetCurrencies", string.Join(",", SelectedTargetCurrencies));
            }
        }

        public async void LoadAlertRulesAsync()
        {
            _alertRules = await App.Database.GetAlertRulesAsync(); // Load alert rules
        }

        public async void FetchExchangeRates()
        {
            var rates = await _exchangeRateService.GetExchangeRatesAsync(SelectedBaseCurrency);
            if (rates == null) return;

            Rates.Clear();
            foreach (var rate in rates)
            {
                if (_selectedTargetCurrencies.Contains(rate.Currency))
                {
                    Rates.Add(rate);  // Add to the ObservableCollection if it's selected

                    //Rates.Add(rate);

                    // Update previous rates
                    if (_previousRates.ContainsKey(rate.Currency))
                        _previousRates[rate.Currency] = rate.Rate;
                    else
                        _previousRates.Add(rate.Currency, rate.Rate);
                }
            }

            CheckAlerts(); // After fetching, immediately check smart alerts
        }

        private void StartTimer()
        {
            _timer = new Timer(60000); // 60 seconds interval
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Start();
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            FetchExchangeRates();
        }

        private void CheckAlerts()
        {
            if (_alertRules == null || !_alertRules.Any()) return;

            foreach (var alert in _alertRules)
            {
                var matchingRate = Rates.FirstOrDefault(r => r.Currency == alert.TargetCurrency);
                if (matchingRate != null)
                {
                    double currentRate = matchingRate.Rate;
                    double previousRate = _previousRates.ContainsKey(alert.TargetCurrency) ? _previousRates[alert.TargetCurrency] : 0;

                    bool alertTriggered = false;

                    if (alert.Condition == ">" && currentRate > alert.TargetValue)
                    {
                        alertTriggered = true;
                    }
                    else if (alert.Condition == "<" && currentRate < alert.TargetValue)
                    {
                        alertTriggered = true;
                    }
                    else if (alert.Condition == "Drop" && previousRate != 0)
                    {
                        double percentChange = ((previousRate - currentRate) / previousRate) * 100;
                        if (percentChange >= alert.TargetValue)
                        {
                            alertTriggered = true;
                        }
                    }

                    if (alertTriggered && !alert.IsTriggered)
                    {
                        alert.IsTriggered = true;
                        _alertService.NotifyAlert(alert); // Show notification (or any UI feedback)
                        App.Database.SaveAlertRuleAsync(alert); // Update alert as triggered
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}