using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Runtime.CompilerServices;

namespace Hangman.ViewModels
{
    public class LetterButtonViewModel : INotifyPropertyChanged
    {
        private bool _isEnabled = true;

        public char Letter { get; }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>Comanda de ghicire - parametrul este litera (char).</summary>
        public ICommand Command { get; }

        public LetterButtonViewModel(char letter, ICommand command)
        {
            Letter = letter;
            Command = command;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CharacterSlotViewModel : INotifyPropertyChanged
    {
        private bool _isRevealed;

        public char Letter { get; }

        public bool IsRevealed
        {
            get => _isRevealed;
            set
            {
                if (_isRevealed != value)
                {
                    _isRevealed = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Display));
                }
            }
        }

        public string Display => IsRevealed ? Letter.ToString() : "_";

        public CharacterSlotViewModel(char letter) => Letter = letter;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class LiveSlotViewModel : INotifyPropertyChanged
    {
        private bool _isUsed;

        public bool IsUsed
        {
            get => _isUsed;
            set
            {
                if (_isUsed != value)
                {
                    _isUsed = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayText));
                }
            }
        }

        public string DisplayText => IsUsed ? "X" : string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
