using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hangman.Services
{
    internal class WordService
    {
        private Dictionary<string, List<string>> _words = new Dictionary<string, List<string>>();
        public const string ALL_CATEGORIES = "All categories";
        public List<string> Categories { get; private set; } = new List<string>();

        public WordService()
        {
            LoadWords();
        }

        private void LoadWords()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "words.json");
            if (!File.Exists(path))
                CreateDefaultWordsFile(path);
            try
            {
                var json = File.ReadAllText(path);
                _words = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json) ?? new Dictionary<string, List<string>>();
            }
            catch
            {
                CreateDefaultWords();
            }
            Categories = _words.Keys.ToList();
        }

        private void CreateDefaultWordsFile(string path)
        {
            CreateDefaultWords();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var json = JsonSerializer.Serialize(_words, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        private void CreateDefaultWords()
        {
            _words = new Dictionary<string, List<string>>
            {
                ["Cars"] = new List<string> { "FERRARI", "LAMBORGHINI", "TOYOTA", "MERCEDES",
                                                   "VOLKSWAGEN", "PORSCHE", "BUGATTI", "MCLAREN" },
                ["Movies"] = new List<string> { "INCEPTION", "TITANIC", "AVATAR", "GLADIATOR",
                                                   "INTERSTELLAR", "MATRIX", "JOKER", "AVENGERS" },
                ["Rivers"] = new List<string> { "AMAZON", "NILE", "DANUBE", "THAMES",
                                                   "VOLGA", "MISSISSIPPI", "RHINE", "JORDAN" },
                ["Animals"] = new List<string> { "ELEPHANT", "GIRAFFE", "PENGUIN", "DOLPHIN",
                                                   "CHEETAH", "KANGAROO", "CROCODILE", "FLAMINGO" },
                ["Countries"] = new List<string> { "ROMANIA", "FRANCE", "GERMANY", "JAPAN",
                                                   "BRAZIL", "AUSTRALIA", "MEXICO", "ARGENTINA" }
            };
        }

        public string GetRandomWord(string category)
        {
            if (category == ALL_CATEGORIES)
            {
                var allWords = _words.Values.SelectMany(w => w).ToList();
                return allWords[new Random().Next(allWords.Count)];
            }
            else if (_words.ContainsKey(category))
            {
                var wordsInCategory = _words[category];
                return wordsInCategory[new Random().Next(wordsInCategory.Count)];
            }
            else
            {
                throw new ArgumentException("Invalid category");
            }
        }
    }
}
