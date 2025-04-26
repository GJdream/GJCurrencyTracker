using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GjCurrencyTracker.Models;
using SQLite;

namespace GjCurrencyTracker.Data
{
    public class LocalDatabase
    {
        private readonly SQLiteAsyncConnection _database;

        public LocalDatabase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<AlertRule>().Wait();

        }
        private readonly string _alertsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "alerts.json");

        private readonly string _settingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "settings.json");

        public async Task SaveAlertsAsync(List<AlertRule> alerts)
        {
            var json = JsonSerializer.Serialize(alerts, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_alertsFilePath, json);
        }

        public Task<List<AlertRule>> GetAlertRulesAsync()
        {
            return _database.Table<AlertRule>().ToListAsync();
        }

        public Task<int> SaveAlertRuleAsync(AlertRule alert)
        {
            if (alert.Id != 0)
                return _database.UpdateAsync(alert);
            else
                return _database.InsertAsync(alert);
        }

        public Task<int> DeleteAlertRuleAsync(AlertRule alert)
        {
            return _database.DeleteAsync(alert);
        }


        public async Task<List<AlertRule>> LoadAlertsAsync()
        {
            if (!File.Exists(_alertsFilePath))
                return new List<AlertRule>();

            var json = await File.ReadAllTextAsync(_alertsFilePath);
            return JsonSerializer.Deserialize<List<AlertRule>>(json) ?? new List<AlertRule>();
        }

        public async Task SaveSelectedBaseCurrencyAsync(string currency)
        {
            var json = JsonSerializer.Serialize(new Dictionary<string, string> { { "BaseCurrency", currency } });
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }

        public async Task<string> LoadSelectedBaseCurrencyAsync()
        {
            if (!File.Exists(_settingsFilePath))
                return "USD";

            var json = await File.ReadAllTextAsync(_settingsFilePath);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return dict != null && dict.ContainsKey("BaseCurrency") ? dict["BaseCurrency"] : "USD";
        }
    }
}