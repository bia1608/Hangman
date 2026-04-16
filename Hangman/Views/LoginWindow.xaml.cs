using Hangman.ViewModels;
using HangmanGame.Models;
using System;
using System.Collections.Generic;
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
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _vm;
        public LoginWindow()
        {
            InitializeComponent();
            _vm = new LoginViewModel();
            DataContext = _vm;

            // Asculta evenimentele de navigatie din ViewModel
            _vm.RequestNewUser += OnRequestNewUser;
            _vm.RequestPlay += OnRequestPlay;
        }

        private void OnRequestNewUser(User _)
        {
            var dialog = new NewUserWindow { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                // Utilizatorul a fost creat, reincarcam lista
                _vm.LoadUsers();
            }
        }

        private void OnRequestPlay(User user)
        {
            var gameWindow = new GameWindow(user) { Owner = this };
            this.Hide();
            gameWindow.ShowDialog();
            this.Show();

            // Reincarcam utilizatorii (un utilizator ar fi putut fi sters in timp ce juca)
            _vm.LoadUsers();
        }
    }
}
