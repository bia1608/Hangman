using HangmanGame.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Hangman.Services
{
    internal class UserService
    {
        private readonly string _filePath;

        public UserService()
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir); // asigură că directorul există
            _filePath = Path.Combine(dataDir, "users.json");
        }

        public List<User> LoadUsers()
        {
            if (!File.Exists(_filePath))
                return new List<User>();
            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
            catch
            {
                return new List<User>();
            }
        }

        private void SaveUsers(List<User> users)
        {
            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        public void AddUser(User user)
        {
            var users = LoadUsers();
            users.Add(user);
            SaveUsers(users);
        }

        public void DeleteUser(string username)
        {
            var users = LoadUsers();
            users.RemoveAll(u => u.Username == username);
            SaveUsers(users);
        }

        public bool UsernameExists(string username)
        {
            var users = LoadUsers();
            return users.Any(u => u.Username == username);
        }
    }
}
