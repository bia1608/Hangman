using System.Collections.Generic;

namespace HangmanGame.Models
{
    public class CategoryStats
    {
        public int Played { get; set; }  // jocuri complete (castigate SAU pierdute)
        public int Won { get; set; }     // jocuri castigate (3 cuvinte ghicite consecutiv)
    }

    public class UserStatistics
    {
        public string Username { get; set; } = string.Empty;

        // Cheie = numele categoriei (ex: "Cars", "All Categories")
        public Dictionary<string, CategoryStats> Categories { get; set; } = new();
    }
}
