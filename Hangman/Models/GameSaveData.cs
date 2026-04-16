using System;
using System.Collections.Generic;

namespace HangmanGame.Models
{
    public class GameSaveData
    {
        public string Username { get; set; } = string.Empty;
        public string SaveName { get; set; } = string.Empty;   // generat automat din timestamp
        public string Category { get; set; } = string.Empty;
        public string Word { get; set; } = string.Empty;       // cuvantul de ghicit (uppercase)
        public List<char> GuessedLetters { get; set; } = new(); // literele deja ghicite
        public int WrongGuesses { get; set; }                  // numarul de greseli curente
        public int TimeRemaining { get; set; }                 // secunde ramase
        public int ConsecutiveWins { get; set; }               // niveluri castigate consecutiv
        public DateTime SavedAt { get; set; }
    }
}
