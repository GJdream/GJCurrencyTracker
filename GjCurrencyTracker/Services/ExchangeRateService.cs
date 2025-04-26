using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GjCurrencyTracker.Models;

namespace GjCurrencyTracker.Services
{
    public class ExchangeRateService
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<List<ExchangeRate>> GetExchangeRatesAsync(string baseCurrency)
        {
            var response = await _httpClient.GetStringAsync($"https://v6.exchangerate-api.com/v6/141d1151550dc84d83169ca4/latest/{baseCurrency}");
            var data = JsonSerializer.Deserialize<ExchangeRateApiResponse>(response);
            var rates = new List<ExchangeRate>();
            foreach (var rate in data.conversion_rates)
            {
                rates.Add(new ExchangeRate { Currency = rate.Key, Rate = rate.Value });
            }
            return rates;
        }

        private class ExchangeRateApiResponse
        {
            public Dictionary<string, double> conversion_rates { get; set; }
        }
    }
}