using GjCurrencyTracker.ViewModels;


namespace GjCurrencyTracker.Views
{

    [QueryProperty("BaseCurrency", "BaseCurrency")]
    [QueryProperty("TargetCurrency", "TargetCurrency")]
    public partial class AlertPage : ContentPage
    {
        public string BaseCurrency { get; set; }
        public string TargetCurrency { get; set; }
        public AlertPage()
        {
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Set the selected currencies when the page appears.
            //SelectedBaseCurrency. = BaseCurrency;
            //TargetCurrencyLabel.Text = TargetCurrency;
        }

        private async void OnBaseCurrencyChanged(object sender, EventArgs e)
        {
            AlertViewModel viewModel = BindingContext as AlertViewModel;
             viewModel.OnCurrencySelectionChanged();
        }

        private void OnTargetCurrencyChanged(object sender, EventArgs e)
        {
            AlertViewModel viewModel = BindingContext as AlertViewModel;
            viewModel.OnCurrencySelectionChanged();
        }
    }
}