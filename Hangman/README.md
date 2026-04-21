# Hangman (WPF .NET) — MVVM

This is a C# **WPF** desktop implementation of the classic **Hangman** game, structured using the **MVVM** pattern (with data binding and commands).

## Requirements

- Windows
- .NET SDK (the project targets `net9.0-windows`)
- Visual Studio 2022 (recommended) or `dotnet` CLI

## How to run

### Visual Studio

1. Open `Hangman.sln`
2. Build & Run (F5)

### .NET CLI

```bash
dotnet restore
dotnet build -c Release
dotnet run
```

The application starts in the login window (`Views/LoginWindow.xaml`).

## Features

- **Login / Start page**
  - Select an existing user
  - Create a new user with an avatar (jpg/jpeg/gif)
  - Delete selected user (also removes user saves + statistics)
- **Game**
  - Categories + **All categories**
  - Guess letters via on-screen buttons or keyboard (A–Z)
  - 30 seconds countdown timer (timeout = loss)
  - 6 wrong guesses maximum
  - Win condition: **3 consecutive words** guessed in the same category
- **Save / Open game**
  - Save at any time and resume later (per user)
- **Statistics**
  - Games played / won per category (for all users)
- **Help → About**
  - Student details window

## Data files & folders

All paths are relative to the application directory (`AppDomain.CurrentDomain.BaseDirectory`).

- `Data/users.json`
  - List of registered users and their avatar path (stored as a **relative path**)
- `Data/words.json`
  - Dictionary of categories → list of words (created with defaults if missing)
- `Data/statistics.json`
  - Per-user statistics (played/won per category)
- `Saves/`
  - Saved games per user (files named like `{username}_{timestamp}.json`)
- `Data/HangmanStages/`
  - Hangman images used during the game: `hangman_0.jpg` … `hangman_6.jpg`

## Project structure

- `Models/`
  - Simple data classes (`User`, `GameSaveData`, `UserStatistics`)
- `Services/`
  - File persistence and domain utilities (`UserService`, `WordService`, `GameSaveService`, `StatisticsService`)
- `ViewModels/`
  - MVVM logic: properties (`INotifyPropertyChanged`), collections (`ObservableCollection`), and commands (`RelayCommand`)
- `Views/`
  - WPF windows (XAML) + minimal code-behind for navigation, menu building, and keyboard handling

