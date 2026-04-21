using Hangman.Services;
using HangmanGame.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Hangman.Views
{
    /// <summary>
    /// Interaction logic for NewUserWindow.xaml
    /// </summary>
    public partial class NewUserWindow : Window
    {
        private readonly UserService _userService;
        private string _selectedImagePath = string.Empty;
        private string _selectedImageFullPath = string.Empty;

        // Lista cai relative catre imaginile predefinite din Data/Images/
        private readonly List<string> _predefinedImages = new();
        private int _currentImageIndex = -1;

        public NewUserWindow()
        {
            InitializeComponent();
            _userService = new UserService();
            LoadPredefinedImages();
        }

        private void LoadPredefinedImages()
        {
            // Incarca imaginile predefinite din folderul Data/Images/
            string imagesDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Images");
            Directory.CreateDirectory(imagesDir);

            var extensions = new[] { "*.jpg", "*.jpeg", "*.gif" };
            foreach (var ext in extensions)
            {
                foreach (var file in Directory.GetFiles(imagesDir, ext))
                {
                    // Stocam calea relativa fata de directorul aplicatiei
                    var relativePath = System.IO.Path.GetRelativePath(
                        AppDomain.CurrentDomain.BaseDirectory, file);
                    _predefinedImages.Add(relativePath);

                    // Afisam in ListBox doar numele fisierului
                    PredefinedImagesList.Items.Add(System.IO.Path.GetFileName(file));
                }
            }

            if (_predefinedImages.Count > 0)
            {
                _currentImageIndex = 0;
                ShowImageByIndex(0);
                PredefinedImagesList.SelectedIndex = 0;
            }
        }

        private void PrevImage_Click(object sender, RoutedEventArgs e)
        {
            if (_predefinedImages.Count == 0) return;
            _currentImageIndex = (_currentImageIndex - 1 + _predefinedImages.Count) % _predefinedImages.Count;
            PredefinedImagesList.SelectedIndex = _currentImageIndex;
            ShowImageByIndex(_currentImageIndex);
        }

        private void NextImage_Click(object sender, RoutedEventArgs e)
        {
            if (_predefinedImages.Count == 0) return;
            _currentImageIndex = (_currentImageIndex + 1) % _predefinedImages.Count;
            PredefinedImagesList.SelectedIndex = _currentImageIndex;
            ShowImageByIndex(_currentImageIndex);
        }

        private void PredefinedImagesList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int idx = PredefinedImagesList.SelectedIndex;
            if (idx < 0) return;
            _currentImageIndex = idx;
            ShowImageByIndex(idx);
        }

        private void ShowImageByIndex(int index)
        {
            if (index < 0 || index >= _predefinedImages.Count) return;

            var relativePath = _predefinedImages[index];
            _selectedImagePath = relativePath;
            SelectedPathText.Text = relativePath;

            _selectedImageFullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            ShowPreview(_selectedImageFullPath);
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Alege o imagine",
                Filter = "Imagini (*.jpg;*.jpeg;*.gif)|*.jpg;*.jpeg;*.gif"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedImageFullPath = dialog.FileName;
                _selectedImagePath = System.IO.Path.GetFileName(_selectedImageFullPath);
                SelectedPathText.Text = _selectedImagePath;
                PredefinedImagesList.SelectedIndex = -1;
                ShowPreview(_selectedImageFullPath);
            }
        }

        private void ShowPreview(string fullPath)
        {
            try
            {
                if (!File.Exists(fullPath)) { PreviewImage.Source = null; return; }
                var img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri(fullPath, UriKind.Absolute);
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                PreviewImage.Source = img;
            }
            catch { PreviewImage.Source = null; }
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();

            // Validari
            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Username-ul nu poate fi gol.", "Eroare",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (username.Contains(' '))
            {
                MessageBox.Show("Username-ul trebuie sa fie un singur cuvant (fara spatii).",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_userService.UsernameExists(username))
            {
                MessageBox.Show($"Username-ul '{username}' este deja folosit.",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Copiem avatarul in folderul aplicatiei astfel incat sa salvam mereu cai relative
            string relativeAvatarPath = string.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(_selectedImageFullPath) && File.Exists(_selectedImageFullPath))
                {
                    var avatarsDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Images", "Avatars");
                    Directory.CreateDirectory(avatarsDir);

                    var ext = System.IO.Path.GetExtension(_selectedImageFullPath).ToLowerInvariant();
                    if (ext is not (".jpg" or ".jpeg" or ".gif"))
                        ext = ".jpg";

                    var safeUsername = string.Concat(username.Where(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '-'));
                    if (string.IsNullOrWhiteSpace(safeUsername)) safeUsername = username;

                    var destFile = System.IO.Path.Combine(avatarsDir, $"{safeUsername}{ext}");
                    File.Copy(_selectedImageFullPath, destFile, overwrite: true);
                    relativeAvatarPath = System.IO.Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, destFile);
                }
            }
            catch
            {
                relativeAvatarPath = string.Empty;
            }

            // Cream si salvam utilizatorul
            var user = new User
            {
                Username = username,
                ImagePath = relativeAvatarPath
            };
            _userService.AddUser(user);

            MessageBox.Show($"Utilizatorul '{username}' a fost creat cu succes!",
                "Succes", MessageBoxButton.OK, MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
