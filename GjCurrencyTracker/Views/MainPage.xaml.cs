using GjCurrencyTracker.ViewModels;
using Microsoft.Maui.Controls;

namespace GjCurrencyTracker.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new MainViewModel();
        }

        private void OnCurrencyCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var currency = checkBox?.BindingContext as string;  // Get the currency from the data context
            var viewModel = BindingContext as MainViewModel;
            if (currency != null)
            {
                if (e.Value)  // If checked
                {
                    if (!viewModel.SelectedTargetCurrencies.Contains(currency))
                    {
                        viewModel.SelectedTargetCurrencies.Add(currency);
                    }
                }
                else  // If unchecked
                {
                    viewModel.SelectedTargetCurrencies.Remove(currency);
                }

                // After the checkbox is toggled, refresh the exchange rates based on selected currencies
                viewModel.FetchExchangeRates();
            }
        }
    }
}