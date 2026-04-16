using Hangman.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.ViewModels
{
    public class StatisticsRowViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost => GamesPlayed - GamesWon;
        public string WinRate => GamesPlayed > 0
            ? $"{(GamesWon * 100.0 / GamesPlayed):F0}%"
            : "N/A";
    }

    public class StatisticsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<StatisticsRowViewModel> Rows { get; } = new();

        public StatisticsViewModel() {
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            var service = new StatisticsService();
            var allStats = service.LoadAll();

            Rows.Clear();
            foreach (var userStats in allStats)
            {
                foreach (var (category, stats) in userStats.Categories)
                {
                    Rows.Add(new StatisticsRowViewModel
                    {
                        Username = userStats.Username,
                        Category = category,
                        GamesPlayed = stats.Played,
                        GamesWon = stats.Won
                    });
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
