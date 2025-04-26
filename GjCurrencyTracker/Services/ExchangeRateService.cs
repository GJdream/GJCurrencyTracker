using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GjCurrencyTracker.Models;
using Newtonsoft.Json;

namespace GjCurrencyTracker.Services
{
    public class ExchangeRateService
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<List<ExchangeRate>> GetExchangeRatesAsync(string baseCurrency)
        {
            if (!string.IsNullOrEmpty(baseCurrency))
            {
                var response = await _httpClient.GetStringAsync($"https://v6.exchangerate-api.com/v6/141d1151550dc84d83169ca4/latest/{baseCurrency}");
                var data = System.Text.Json.JsonSerializer.Deserialize<ExchangeRateApiResponse>(response);
                var rates = new List<ExchangeRate>();
                foreach (var rate in data.conversion_rates)
                {
                    rates.Add(new ExchangeRate { baseCurrency = baseCurrency, Currency = rate.Key, Rate = rate.Value });
                }
                return rates;
            }
            else
            {
                return new List<ExchangeRate>();
            }
        }

        public async Task<double> GetExchangeRateAsync(string baseCurrency, string targetCurrency)
        {
            var rates = await GetExchangeRatesAsync(baseCurrency);

            var targetRate = rates.FirstOrDefault(r => r.Currency == targetCurrency);
            if (targetRate != null)
            {
                return targetRate.Rate;
            }

            throw new Exception($"Exchange rate from {baseCurrency} to {targetCurrency} not found.");
        }

        public class ExchangeRateApiResponse
        {
            public Dictionary<string, double> conversion_rates { get; set; }
        }

        public async Task<List<HistoricalRate>> GetHistoricalRatesAsync(string baseCurrency, string targetCurrency)
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-7);
           // var url = $"https://api.exchangerate.host/timeseries?start_date=2024-04-01&end_date=2024-04-25&base={baseCurrency}&symbols={targetCurrency}";
            var url = $"https://api.exchangerate.host/timeseries?start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}&base={baseCurrency}&symbols={targetCurrency}";
            var client = new HttpClient();
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<HistoricalApiResponse>(json);

                if (result.Rates != null)
                {
                    var historyList = new List<HistoricalRate>();

                    foreach (var item in result.Rates)
                    {
                        historyList.Add(new HistoricalRate
                        {
                            Date = DateTime.Parse(item.Key),
                            Rate = item.Value[targetCurrency]
                        });
                    }

                    return historyList;
                }
                else
                {
                    return LoadRandomData(baseCurrency,targetCurrency);
                    //return new List<HistoricalRate>();
                }
            }
            else
            {
                 return new List<HistoricalRate>();
            }
        }

        private List<HistoricalRate> LoadRandomData(string baseCurrency , string targetCurrency)
        {
           var historicalRates  = new List<HistoricalRate>();

            if (baseCurrency == targetCurrency)
                return historicalRates;

            //if (baseCurrency == "USD" && targetCurrency == "INR")
            //{
                Random rand = new Random();
                double startingRate = GetStartingRate(baseCurrency, targetCurrency);
                double rateFluctuation = startingRate * 0.03;  // Rate can fluctuate by 3%

                // Simulate data for the last 10 days
                for (int i = 0; i < 7; i++)
                {
                    var newRate = startingRate + (rand.NextDouble() * rateFluctuation * 2 - rateFluctuation);  // Fluctuates rate by +/- 3%
                historicalRates.Add(new HistoricalRate
                    {
                        Date = DateTime.Today.AddDays(-i),
                        Rate = Math.Round(newRate, 4)  // Round to 4 decimal places
                    });

                    // Prepare for the next day (slightly adjust the rate)
                    startingRate = newRate;
                }
            
            return historicalRates;
        }

        private double GetStartingRate(string baseCurrency, string targetCurrency)
        {
            if (baseCurrency == "USD" && targetCurrency == "INR") return 83.0;
            if (baseCurrency == "USD" && targetCurrency == "EUR") return 0.92;
            if (baseCurrency == "USD" && targetCurrency == "JPY") return 154.0;

            if (baseCurrency == "EUR" && targetCurrency == "USD") return 1.08;
            if (baseCurrency == "EUR" && targetCurrency == "INR") return 90.0;
            if (baseCurrency == "EUR" && targetCurrency == "JPY") return 167.0;

            if (baseCurrency == "INR" && targetCurrency == "USD") return 0.012;
            if (baseCurrency == "INR" && targetCurrency == "EUR") return 0.011;
            if (baseCurrency == "INR" && targetCurrency == "JPY") return 1.85;

            if (baseCurrency == "JPY" && targetCurrency == "USD") return 0.0065;
            if (baseCurrency == "JPY" && targetCurrency == "EUR") return 0.0060;
            if (baseCurrency == "JPY" && targetCurrency == "INR") return 0.54;

            return 1.0; // Default fallback (shouldn't happen)
        }

    }
}