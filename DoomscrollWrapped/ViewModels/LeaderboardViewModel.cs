using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DoomscrollWrapped.Models;
using DoomscrollWrapped.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DoomscrollWrapped.ViewModels
{
    public partial class LeaderboardViewModel : ObservableObject
    {
        private readonly IDailyLogService _dailyLogService;

        public ObservableCollection<LeaderboardEntry> Entries { get; } = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _hasEntries;

        [ObservableProperty]
        private bool _isEmpty;

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private bool _hasError;

        public LeaderboardViewModel(IDailyLogService dailyLogService)
        {
            _dailyLogService = dailyLogService;
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            IsLoading = true;
            HasError = false;
            HasEntries = false;
            IsEmpty = false;
            ErrorMessage = null;
            Entries.Clear();

            try
            {
                var results = await _dailyLogService.GetGlobalLeaderboardAsync();

                foreach (var entry in results)
                {
                    Entries.Add(entry);
                }

                HasEntries = Entries.Count > 0;
                IsEmpty = Entries.Count == 0;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not load leaderboard: {ex.Message}";
                HasError = true;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
