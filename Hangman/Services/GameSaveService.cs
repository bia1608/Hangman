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
    internal class GameSaveService
    {
        private readonly string _saveDir;

        public GameSaveService()
        {
            _saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");
            if (!Directory.Exists(_saveDir))
            {
                Directory.CreateDirectory(_saveDir);
            }
        }

        public void SaveGame(GameSaveData data)
        {
            data.SavedAt = DateTime.Now;
            data.SaveName = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"{data.Username}_{data.SaveName}.json";
            var path = Path.Combine(_saveDir, filename);
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public List<GameSaveData> LoadSavedGames(string username)
        {
            var saves = new List<GameSaveData>();
            foreach(var file in Directory.GetFiles(_saveDir, $"{username}_*.json"))
            {
                var json = File.ReadAllText(file);
                var data = JsonSerializer.Deserialize<GameSaveData>(json);
                if (data != null && data.Username == username)
                {
                    saves.Add(data);
                }
            }
            saves.Sort((a, b) => b.SavedAt.CompareTo(a.SavedAt)); // Most recent first
            return saves;
        }

        public void DeleteUserSaves(string username)
        {
            if(!Directory.Exists(_saveDir))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(_saveDir, $"{username}_*.json"))
            {
                File.Delete(file);
            }
        }
    }
}
