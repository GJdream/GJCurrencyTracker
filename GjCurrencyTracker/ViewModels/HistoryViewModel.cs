using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using GjCurrencyTracker.Models;
using GjCurrencyTracker.Services;
using GjCurrencyTracker.Views;

namespace GjCurrencyTracker.ViewModels
{
    public class HistoryViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> AvailableCurrencies { get; set; } = new() { "USD", "EUR", "INR", "JPY" };
        private readonly ExchangeRateService _exchangeRateService;

        private string _baseCurrency;
        private string _targetCurrency;

        //public ObservableCollection<HistoricalRate> HistoricalRates { get; set; } = new ObservableCollection<HistoricalRate>();

        private ObservableCollection<HistoricalRate> _historicalRates = new ObservableCollection<HistoricalRate>();
        public ObservableCollection<HistoricalRate> HistoricalRates
        {
            get => _historicalRates;
            set
            {
                _historicalRates = value;
                OnPropertyChanged(nameof(HistoricalRates)); // <-- VERY important
            }
        }

        public string BaseCurrency
        {
            get => _baseCurrency;
            set
            {
                if (_baseCurrency != value)
                {
                    _baseCurrency = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TargetCurrency
        {
            get => _targetCurrency;
            set
            {
                if (_targetCurrency != value)
                {
                    _targetCurrency = value;
                    OnPropertyChanged();
                }
            }
        }
        public ICommand LoadHistoryCommand { get; }

        public HistoryViewModel()
        {
            _exchangeRateService = new ExchangeRateService();

            BaseCurrency = AvailableCurrencies.First();
            TargetCurrency = AvailableCurrencies.Skip(1).First();
            LoadHistoricalRatesAsync();
            LoadHistoryCommand = new Command(async () => await LoadHistoricalRatesAsync());
    }

        public async Task LoadHistoricalRatesAsync()
        {
            if (string.IsNullOrWhiteSpace(BaseCurrency) || string.IsNullOrWhiteSpace(TargetCurrency))
                return;

            var historicalData = await _exchangeRateService.GetHistoricalRatesAsync(BaseCurrency, TargetCurrency);

            HistoricalRates.Clear();
            foreach (var rate in historicalData)
            {
                HistoricalRates.Add(rate);
            }

            HistoryPage.Current?.ReloadChart();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
