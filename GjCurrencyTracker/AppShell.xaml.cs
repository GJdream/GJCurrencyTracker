using GjCurrencyTracker.Views;

namespace GjCurrencyTracker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(AlertPage), typeof(AlertPage));
        }
    }
}
