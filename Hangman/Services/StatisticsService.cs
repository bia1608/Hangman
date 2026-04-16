using HangmanGame.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hangman.Services
{
    internal class StatisticsService
    {
        private readonly string _filePath;

        public StatisticsService()
        {
            var dir = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dir);
            _filePath = Path.Combine(dir, "statistics.json");
        }

        public List<UserStatistics> LoadAll()
        {
            if (!File.Exists(_filePath)) return new List<UserStatistics>();
            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<UserStatistics>>(json) ?? new();
            }
            catch { return new List<UserStatistics>(); }
        }

        private void SaveAll(List<UserStatistics> stats)
        {
            var json = JsonSerializer.Serialize(stats, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        public void RecordGameResults(string username,string category, bool won)
        {
            var all =  LoadAll();
            var userStats = all.FirstOrDefault(u => u.Username == username);
            if (userStats == null)
            {
                userStats = new UserStatistics
                {
                    Username = username,
                    Categories = new Dictionary<string, CategoryStats>()
                };
                all.Add(userStats);
            }

            if (!userStats.Categories.ContainsKey(category))
                userStats.Categories[category] = new CategoryStats();

            userStats.Categories[category].Played++;
            if(won)
                userStats.Categories[category].Won++;

            SaveAll(all);
        }

        public void DeleteUserStats(string username)
        {
            var all = LoadAll();
            all.RemoveAll(u => u.Username == username);
            SaveAll(all);
        }
    }
}
