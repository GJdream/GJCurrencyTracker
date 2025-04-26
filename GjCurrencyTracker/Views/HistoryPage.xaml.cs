using Microcharts;
using SkiaSharp;
using Microcharts.Maui;
using GjCurrencyTracker.ViewModels;

namespace GjCurrencyTracker.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HistoryPage : ContentPage
    {
        //public HistoryPage()
        //{
        //    InitializeComponent();
        //    LoadChart();
        //}

        //private void LoadChart()
        //{
        //    var entries = new List<ChartEntry>
        //    {
        //        new ChartEntry(1.05f)
        //        {
        //            Label = "Day 1",
        //            ValueLabel = "1.05",
        //            Color = SKColor.Parse("#1E88E5"),
        //            TextColor = SKColors.Black
        //        },
        //        new ChartEntry(1.08f)
        //        {
        //            Label = "Day 2",
        //            ValueLabel = "1.08",
        //            Color = SKColor.Parse("#43A047"),
        //            TextColor = SKColors.Black
        //        },
        //        new ChartEntry(1.02f)
        //        {
        //            Label = "Day 3",
        //            ValueLabel = "1.02",
        //            Color = SKColor.Parse("#E53935"),
        //            TextColor = SKColors.Black
        //        },
        //        new ChartEntry(1.10f)
        //        {
        //            Label = "Day 4",
        //            ValueLabel = "1.10",
        //            Color = SKColor.Parse("#FB8C00"),
        //            TextColor = SKColors.Black
        //        }
        //    };

        //    this.chartView.Chart = new LineChart
        //    {
        //        Entries = entries,
        //        LineMode = LineMode.Straight, // or Spline for smooth curves
        //        LineSize = 8,
        //        PointSize = 18,
        //        BackgroundColor = SKColors.Transparent,
        //        LabelTextSize = 30,
        //        AnimationDuration = TimeSpan.FromSeconds(1),
        //        ValueLabelOption = ValueLabelOption.TopOfChart,
        //        ValueLabelOrientation = Orientation.Horizontal
        //    };
        //}

        public static HistoryPage Current { get; private set; }
        private readonly HistoryViewModel _viewModel;

        public HistoryPage()
        {
            InitializeComponent();
            _viewModel = new HistoryViewModel();
            BindingContext = _viewModel;
            Current = this;
            // Set default currencies (you can also set dynamically)
            _viewModel.BaseCurrency = "USD";
            _viewModel.TargetCurrency = "INR";

            LoadChartAsync();
        }

        private async Task LoadChartAsync()
        {
            ChartViewControl.Chart = null; // clear previous chart if needed

            await _viewModel.LoadHistoricalRatesAsync();  // loads based on current base/target currencies

            var entries = _viewModel.HistoricalRates.Select(rate => new ChartEntry((float)rate.Rate)
            {
                Label = rate.Date.ToString("MM/dd"),
                ValueLabel = rate.Rate.ToString("F2"),
                Color = SKColor.Parse("#3498db")
            }).ToList();

            ChartViewControl.Chart = new LineChart
            {
                Entries = entries,
                LineMode = LineMode.Straight,
                LineSize = 6,
                PointMode = PointMode.Circle,
                PointSize = 10,
                BackgroundColor = SKColors.White,
                LabelTextSize = 30
            };
        }

        public async void ReloadChart()
        {
            var entries = _viewModel.HistoricalRates.Select(rate => new ChartEntry((float)rate.Rate)
            {
                Label = rate.Date.ToString("MM/dd"),
                ValueLabel = rate.Rate.ToString("F2"),
                Color = SKColor.Parse("#3498db")
            }).ToList();

            ChartViewControl.Chart = new LineChart
            {
                Entries = entries,
                LineMode = LineMode.Straight,
                LineSize = 6,
                PointMode = PointMode.Circle,
                PointSize = 10,
                BackgroundColor = SKColors.White,
                LabelTextSize = 30
            };
        }
    }
}