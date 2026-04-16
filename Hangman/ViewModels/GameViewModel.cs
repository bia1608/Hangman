using Hangman.Services;
using HangmanGame.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Hangman.ViewModels
{
    internal class GameViewModel : INotifyPropertyChanged
    {
        private readonly WordService _wordService;
        private readonly GameSaveService _saveService;
        private readonly StatisticsService _statsService;

        public const int MaxWrongGuesses = 6;
        public const int GameTimerSeconds = 30;

        private readonly DispatcherTimer _timer;
        private string _currentWord = string.Empty;
        private int _wrongGuesses;
        private int _timeRemaining;
        private int _consecutiveWins;
        private bool _isGameActive;
        private string _currentCategory;
        private string _statusMessage = "Incepe un joc nou: File → New Game";

        public User CurrentUser { get; }
        private BitmapImage? _userImage;

        public BitmapImage? UserImage
        {
            get => _userImage;
            private set
            {
                _userImage = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<CharacterSlotViewModel> WordDisplay { get; } = new();
        public ObservableCollection<LetterButtonViewModel> LetterButtons { get; } = new();
        public ObservableCollection<LiveSlotViewModel> Lives { get; } = new();

        public int WrongGuesses
        {
            get => _wrongGuesses;
            private set
            {
                _wrongGuesses = value;
                OnPropertyChanged();
                HangmanStageChanged?.Invoke(value);
            }
        }

        public int TimeRemaining
        {
            get => _timeRemaining;
            private set
            {
                _timeRemaining = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimerDisplay));
            }
        }

        public int ConsecutiveWins
        {
            get => _consecutiveWins;
            private set
            {
                _consecutiveWins = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LevelDisplay));
            }
        }

        public bool IsGameActive
        {
            get => _isGameActive;
            private set
            {
                _isGameActive = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string CurrentCategory
        {
            get => _currentCategory;
            set
            {
                _currentCategory = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public string TimerDisplay => $"Timp: {TimeRemaining}s";
        public string LevelDisplay => $"Nivel: {ConsecutiveWins}/3";

        public WordService WordService => _wordService;

        // Event: View-ul asculta si redeseneaza Canvas-ul spanzuratorii
        public event Action<int>? HangmanStageChanged;
        // Events de navigatie (code-behind asculta si deschide ferestele corespunzatoare)
        public event Action? RequestCancel;
        public event Action? RequestStatistics;
        public event Action? RequestAbout;
        public event Action<List<GameSaveData>>? RequestOpenGame;

        // ---- Comenzi (Comanda 1: New Game, 2: Save, 3: Cancel etc.) ----
        public ICommand NewGameCommand { get; }
        public ICommand OpenGameCommand { get; }
        public ICommand SaveGameCommand { get; }
        public ICommand StatisticsCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AboutCommand { get; }

        public GameViewModel(User user)
        {
            CurrentUser = user;
            _currentCategory = WordService.ALL_CATEGORIES;
            _wordService = new WordService();
            _saveService = new GameSaveService();
            _statsService = new StatisticsService();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnTimerTick;

            NewGameCommand = new RelayCommand(_ => StartNewGame(resetLevel: true));
            OpenGameCommand = new RelayCommand(_ => OpenSavedGame());
            SaveGameCommand = new RelayCommand(_ => SaveCurrentGame(), _ => IsGameActive);
            StatisticsCommand = new RelayCommand(_ => RequestStatistics?.Invoke());
            CancelCommand = new RelayCommand(_ => CancelGame());
            AboutCommand = new RelayCommand(_ => RequestAbout?.Invoke());

            InitializeLives();
            InitializeLetterButtons();
            LoadUserImage();
        }

        // ================================================================
        //  Initializare
        // ================================================================

        private void InitializeLives()
        {
            Lives.Clear();
            for (int i = 0; i < MaxWrongGuesses; i++)
                Lives.Add(new LiveSlotViewModel());
        }

        private void InitializeLetterButtons()
        {
            LetterButtons.Clear();
            for (char c = 'A'; c <= 'Z'; c++)
            {
                char letter = c;
                LetterButtons.Add(new LetterButtonViewModel(
                    letter,
                    new RelayCommand(_ => GuessLetter(letter))));
            }
        }

        private void LoadUserImage()
        {
            if (string.IsNullOrEmpty(CurrentUser.ImagePath)) return;
            try
            {
                var fullPath = Path.IsPathRooted(CurrentUser.ImagePath)
                    ? CurrentUser.ImagePath
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CurrentUser.ImagePath);
                if (!File.Exists(fullPath)) return;
                var img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri(fullPath, UriKind.Absolute);
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                UserImage = img;
                OnPropertyChanged(nameof(UserImage));
            }
            catch { }
        }

        // ================================================================
        //  Logica joc
        // ================================================================

        /// <param name="resetLevel">
        /// true  = joc nou din meniu / schimbare categorie → reseteaza nivelurile la 0.
        /// false = avansare automata dupa castigarea unui nivel → pastreaza ConsecutiveWins.
        /// </param>
        public void StartNewGame(bool resetLevel)
        {
            _timer.Stop();
            if (resetLevel) ConsecutiveWins = 0;

            WrongGuesses = 0;          // declanseza si HangmanStageChanged(0)
            TimeRemaining = GameTimerSeconds;
            ResetLives();
            EnableAllLetterButtons();

            _currentWord = _wordService.GetRandomWord(CurrentCategory);
            BuildWordDisplay(_currentWord);

            IsGameActive = true;
            StatusMessage = $"Ghiceste cuvantul! Nivel {ConsecutiveWins + 1}/3 | {CurrentCategory}";
            _timer.Start();
        }

        private void BuildWordDisplay(string word)
        {
            WordDisplay.Clear();
            foreach (char c in word)
                WordDisplay.Add(new CharacterSlotViewModel(c));
        }

        public void GuessLetter(char letter)
        {
            if (!IsGameActive) return;

            var btn = LetterButtons.FirstOrDefault(b => b.Letter == letter);
            if (btn != null) btn.IsEnabled = false;

            bool found = false;
            for (int i = 0; i < _currentWord.Length; i++)
                if (_currentWord[i] == letter)
                { WordDisplay[i].IsRevealed = true; found = true; }

            if (!found)
            {
                Lives[WrongGuesses].IsUsed = true;
                WrongGuesses++;
                if (WrongGuesses >= MaxWrongGuesses) OnWordLost();
            }
            else if (WordDisplay.All(s => s.IsRevealed))
            {
                OnWordWon();
            }
        }

        private void OnWordWon()
        {
            _timer.Stop();
            int reachedLevel = ConsecutiveWins + 1;

            if (reachedLevel >= 3)
            {
                ConsecutiveWins = 3;
                IsGameActive = false;
                _statsService.RecordGameResults(CurrentUser.Username, CurrentCategory, won: true);
                StatusMessage = "Felicitari! Ai castigat jocul!";

                MessageBox.Show(
                    $"Felicitari, {CurrentUser.Username}!\n\n" +
                    $"Ai ghicit 3 cuvinte consecutive in categoria '{CurrentCategory}'.\n" +
                    "Poti incepe un nou joc din File → New Game.",
                    "Joc Castigat!", MessageBoxButton.OK, MessageBoxImage.Information);

                ConsecutiveWins = 0;
            }
            else
            {
                ConsecutiveWins = reachedLevel;
                StatusMessage = $"Corect! Nivel {ConsecutiveWins}/3 trecut. Urmatorul cuvant...";

                MessageBox.Show(
                    $"Corect! Cuvantul era '{_currentWord}'.\n" +
                    $"Nivel {ConsecutiveWins}/3 trecut. Continua!",
                    "Nivel Trecut!", MessageBoxButton.OK, MessageBoxImage.Information);

                // Avansam automat la nivelul urmator (ConsecutiveWins este deja setat)
                StartNewGame(resetLevel: false);
            }
        }

        private void OnWordLost()
        {
            _timer.Stop();
            IsGameActive = false;

            foreach (var slot in WordDisplay) slot.IsRevealed = true;
            _statsService.RecordGameResults(CurrentUser.Username, CurrentCategory, won: false);

            int prevWins = ConsecutiveWins;
            ConsecutiveWins = 0;

            string reason = WrongGuesses >= MaxWrongGuesses
                ? "Ai epuizat toate incercarile!"
                : "Timpul a expirat!";

            StatusMessage = $"Joc pierdut! Cuvantul era: {_currentWord}";

            MessageBox.Show(
                $"{reason}\n\nCuvantul era: '{_currentWord}'" +
                (prevWins > 0 ? $"\n(Ai acumulat {prevWins} nivel(uri) inainte de aceasta greseala.)" : ""),
                "Joc Pierdut", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            TimeRemaining--;
            if (TimeRemaining <= 0) { TimeRemaining = 0; OnWordLost(); }
        }

        // ================================================================
        //  Salvare / Incarcare
        // ================================================================

        private void SaveCurrentGame()
        {
            if (!IsGameActive) return;
            _timer.Stop();

            _saveService.SaveGame(new GameSaveData
            {
                Username = CurrentUser.Username,
                Category = CurrentCategory,
                Word = _currentWord,
                GuessedLetters = LetterButtons.Where(b => !b.IsEnabled).Select(b => b.Letter).ToList(),
                WrongGuesses = WrongGuesses,
                TimeRemaining = TimeRemaining,
                ConsecutiveWins = ConsecutiveWins
            });

            _timer.Start();
            MessageBox.Show("Jocul a fost salvat cu succes!", "Salvat",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenSavedGame()
        {
            _timer.Stop();
            var saves = _saveService.LoadSavedGames(CurrentUser.Username);

            if (saves.Count == 0)
            {
                MessageBox.Show("Nu ai niciun joc salvat.", "Open Game",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                if (IsGameActive) _timer.Start();
                return;
            }

            RequestOpenGame?.Invoke(saves);
        }

        public void RestoreSavedGame(GameSaveData data)
        {
            _timer.Stop();
            IsGameActive = false;
            CurrentCategory = data.Category;
            _currentWord = data.Word;
            ConsecutiveWins = data.ConsecutiveWins;
            TimeRemaining = data.TimeRemaining;

            ResetLives();
            BuildWordDisplay(_currentWord);
            EnableAllLetterButtons();

            foreach (var letter in data.GuessedLetters)
            {
                var btn = LetterButtons.FirstOrDefault(b => b.Letter == letter);
                if (btn != null) btn.IsEnabled = false;
                for (int i = 0; i < _currentWord.Length; i++)
                    if (_currentWord[i] == letter) WordDisplay[i].IsRevealed = true;
            }

            for (int i = 0; i < data.WrongGuesses && i < MaxWrongGuesses; i++)
                Lives[i].IsUsed = true;

            _wrongGuesses = data.WrongGuesses;
            OnPropertyChanged(nameof(WrongGuesses));
            HangmanStageChanged?.Invoke(_wrongGuesses);

            IsGameActive = true;
            StatusMessage = $"Joc restaurat! {CurrentCategory} | Nivel {ConsecutiveWins}/3";
            _timer.Start();
        }

        // ================================================================
        //  Schimbare categorie si Cancel
        // ================================================================

        public void ChangeCategory(string category)
        {
            _timer.Stop();
            CurrentCategory = category;
            ConsecutiveWins = 0;
            IsGameActive = false;
            StatusMessage = $"Categorie: {category}. File → New Game pentru a incepe.";
        }

        private void CancelGame()
        {
            if (IsGameActive)
            {
                var res = MessageBox.Show("Parasesti jocul curent?",
                    "Confirmare", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res != MessageBoxResult.Yes) return;
            }
            _timer.Stop();
            RequestCancel?.Invoke();
        }

        public void OnWindowClosing() => _timer.Stop();

        /// <summary>Reia timer-ul dupa ce un dialog (ex: Open Game) a fost anulat.</summary>
        public void ResumeTimer() { if (IsGameActive && !_timer.IsEnabled) _timer.Start(); }

        private void ResetLives() { foreach (var l in Lives) l.IsUsed = false; }
        private void EnableAllLetterButtons() { foreach (var b in LetterButtons) b.IsEnabled = true; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}