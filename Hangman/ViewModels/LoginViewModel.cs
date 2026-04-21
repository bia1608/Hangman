using Hangman.Services;
using HangmanGame.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Hangman.ViewModels
{
    internal class LoginViewModel : INotifyPropertyChanged
    {
        private readonly UserService _userService;
        private readonly StatisticsService _statsService;
        private readonly GameSaveService _saveService;

        private User? _selectedUser;
        private BitmapImage? _selectedUserImage;

        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();

        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                LoadUserImage(value?.ImagePath);
                OnPropertyChanged(nameof(IsUserSelected));
                CommandManager.InvalidateRequerySuggested();
                OnPropertyChanged();
            }
        }

        public BitmapImage? SelectedUserImage
        {
            get => _selectedUserImage;
            private set
            {
                _selectedUserImage = value;
                OnPropertyChanged();
            }
        }

        public bool IsUserSelected => SelectedUser != null; // pt butoanele Delete/Play

        public ICommand NewUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<User>? RequestPlay;
        public event Action<User> RequestNewUser;

        public LoginViewModel()
        {
            _userService = new UserService();
            _statsService = new StatisticsService();
            _saveService = new GameSaveService();

            NewUserCommand = new RelayCommand(_ => OnRequestNewUser());
            DeleteUserCommand = new RelayCommand(_ => DeleteSelectedUser(), _ => IsUserSelected);
            PlayCommand = new RelayCommand(_ => OnRequestPlay(), _ => IsUserSelected);
            CancelCommand = new RelayCommand(_ => Application.Current.Shutdown());

            LoadUsers();
        }

        public void LoadUsers()
        {
            Users.Clear();
            foreach(var u in _userService.LoadUsers())
                Users.Add(u);
        }

        private void OnRequestNewUser() 
            => RequestNewUser?.Invoke(new User());

        private void OnRequestPlay()
        {
            if (SelectedUser != null)
                RequestPlay?.Invoke(SelectedUser);
        }

        private void DeleteSelectedUser()
        {
            if(SelectedUser == null)
                return;
            var result = MessageBox.Show($"Are you sure you want to delete user '{SelectedUser.Username}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if(result != MessageBoxResult.Yes)
                return;

            var username = SelectedUser.Username;
            _userService.DeleteUser(username);
            _statsService.DeleteUserStats(username);
            _saveService.DeleteUserSaves(username);

            SelectedUser = null;
            LoadUsers();
        }

        private void LoadUserImage(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                SelectedUserImage = null;
                return;
            }

            try
            {
                var fullPath = Path.IsPathRooted(imagePath)
                    ? imagePath
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);

                if (!File.Exists(fullPath))
                {
                    SelectedUserImage = null;
                    return;
                }

                var img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri(fullPath, UriKind.Absolute);
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                SelectedUserImage = img;
            }
            catch
            {
                SelectedUserImage = null;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
