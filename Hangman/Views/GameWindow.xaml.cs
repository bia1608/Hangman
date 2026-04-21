using Hangman.Services;
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
    /// <summary>
    /// Code-behind pentru fereastra de joc.
    ///
    /// Responsabilitati (ce NU poate face ViewModel-ul fiindca nu stie de WPF drawing):
     ///   1. Generarea dinamica a meniului Categories.
     ///   2. Gestionarea apasarilor de taste pentru litere (A-Z).
     ///   3. Afisarea dialogurilor de navigatie (statistici, about, open game).
    /// </summary>
    public partial class GameWindow : Window
    {
        private readonly GameViewModel _vm;

        public GameWindow(User user)
        {
            InitializeComponent();
            _vm = new GameViewModel(user);
            DataContext = _vm;

            // Conectam evenimentele din ViewModel la handlere locale
            _vm.RequestCancel += OnRequestCancel;
            _vm.RequestStatistics += OnRequestStatistics;
            _vm.RequestAbout += OnRequestAbout;
            _vm.RequestOpenGame += OnRequestOpenGame;

            BuildCategoriesMenu();
        }

        private void BuildCategoriesMenu()
        {
            CategoriesMenu.Items.Clear();

            // Adaugam "All Categories" primul
            var allItem = new MenuItem { Header = WordService.ALL_CATEGORIES };
            allItem.Click += CategoryItem_Click;

            // Marcam categoria curenta ca selectata
            allItem.IsChecked = (_vm.CurrentCategory == WordService.ALL_CATEGORIES);
            CategoriesMenu.Items.Add(allItem);

            CategoriesMenu.Items.Add(new Separator());

            foreach (var category in _vm.WordService.Categories)
            {
                var item = new MenuItem { Header = category, Tag = category };
                item.IsChecked = (_vm.CurrentCategory == category);
                item.Click += CategoryItem_Click;
                CategoriesMenu.Items.Add(item);
            }
        }

        private void CategoryItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem clickedItem) return;

            string category = clickedItem.Tag as string ?? WordService.ALL_CATEGORIES;

            // Bifam doar categoria selectata in meniu
            foreach (var menuItemObj in CategoriesMenu.Items)
            {
                if (menuItemObj is MenuItem mi)
                    mi.IsChecked = (mi.Tag as string == category ||
                                   (category == WordService.ALL_CATEGORIES &&
                                    mi.Tag == null));
            }

            _vm.ChangeCategory(category);
        }

        // ================================================================
        //  Scurtaturi tastatura pentru litere A-Z
        // ================================================================

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Ignoram modificatori (Ctrl+N etc sunt tratate prin InputBinding)
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                return;

            // Convertim Key in litera A-Z
            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                char letter = (char)('A' + (e.Key - Key.A));
                _vm.GuessLetter(letter);
                e.Handled = true;
            }
        }

        // ================================================================
        //  Navigatie
        // ================================================================

        private void OnRequestCancel()
        {
            DialogResult = false;
            Close();
        }

        private void OnRequestStatistics()
        {
            new StatisticsWindow { Owner = this }.ShowDialog();
        }

        private void OnRequestAbout()
        {
            new AboutWindow { Owner = this }.ShowDialog();
        }

        /// <summary>
        /// Afiseaza un dialog simplu cu lista jocurilor salvate.
        /// Userul alege unul si ViewModel-ul il restaureaza.
        /// </summary>
        private void OnRequestOpenGame(List<GameSaveData> saves)
        {
            var dialog = new OpenGameWindow(saves) { Owner = this };
            if (dialog.ShowDialog() == true && dialog.SelectedSave != null)
            {
                _vm.RestoreSavedGame(dialog.SelectedSave);
            }
            else
            {
                // Userul a anulat dialogul; reluam timer-ul daca jocul era activ
                _vm.ResumeTimer();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _vm.OnWindowClosing();
        }
    }
}
