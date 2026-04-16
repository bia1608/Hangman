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
    /// Interaction logic for OpenGameWindow.xaml
    /// </summary>
    public partial class OpenGameWindow : Window
    {
        public GameSaveData? SelectedSave { get; private set; }

        public OpenGameWindow(List<GameSaveData> saves)
        {
            InitializeComponent();
            SavesList.ItemsSource = saves;
            if (saves.Count > 0) SavesList.SelectedIndex = 0;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            SelectedSave = SavesList.SelectedItem as GameSaveData;
            if (SelectedSave == null)
            {
                MessageBox.Show("Selecteaza un joc din lista.", "Atentie",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
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
