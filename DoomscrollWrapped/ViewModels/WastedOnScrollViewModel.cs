using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DoomscrollWrapped.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DoomscrollWrapped.ViewModels
{
    public partial class WastedOnScrollViewModel : ObservableObject
    {
        private readonly IDailyLogService _dailyLogService;

        [ObservableProperty]
        private string _tikTokText = "TikTok: Clean! ✨";

        [ObservableProperty]
        private string _instagramText = "Instagram: Clean! ✨";

        [ObservableProperty]
        private string _youTubeText = "YouTube: Clean! ✨";

        [ObservableProperty]
        private string _totalTimeText = "For a whopping total of: 0 hours!";

        [ObservableProperty]
        private string _booksText = "You could have read 0.0 books.";

        [ObservableProperty]
        private string _moneyText = "You lost out on 0.00 zł (at 31.4/hr).";

        [ObservableProperty]
        private string _gymText = "You missed 0.0 gym sessions.";

        [ObservableProperty]
        private string _skillText = "You could learn 0.0000% of new skill.";

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isResultsVisible;

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private bool _hasError;

        public WastedOnScrollViewModel(IDailyLogService dailyLogService)
        {
            _dailyLogService = dailyLogService;
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsLoading = true;
            IsResultsVisible = false;
            ErrorMessage = null;
            HasError = false;

            try
            {
                IReadOnlyDictionary<string, int> stats = await _dailyLogService.GetAllTimeMinutesByAppAsync();

                if (stats.Count == 0)
                {
                    ApplyZeroStats();
                }
                else
                {
                    int tiktok = stats.TryGetValue("TikTok", out int tiktokMinutes) ? tiktokMinutes : 0;
                    int instagram = stats.TryGetValue("Instagram", out int instagramMinutes) ? instagramMinutes : 0;
                    int youtube = stats.TryGetValue("YouTube", out int youtubeMinutes) ? youtubeMinutes : 0;
                    int totalMinutes = tiktok + instagram + youtube;

                    ApplyStats(tiktok, instagram, youtube, totalMinutes);
                }

                IsResultsVisible = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not load stats: {ex.Message}";
                HasError = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyStats(int tiktok, int instagram, int youtube, int totalMinutes)
        {
            double hours = totalMinutes / 60.0;

            TikTokText = FormatAppText("TikTok", tiktok);
            InstagramText = FormatAppText("Instagram", instagram);
            YouTubeText = FormatAppText("YouTube", youtube);
            TotalTimeText = $"For a whopping total of: {Math.Round(hours, 1)} hours!";

            BooksText = $"You could have read {(hours / 5.0):F1} books.";
            MoneyText = $"You lost out on {(hours * 31.4):F2} zł (at 31.4/hr).";
            GymText = $"You missed {(hours / 1.5):F1} gym sessions.";
            SkillText = $"You could learn {(hours / 20.0) * 100.0:F4}% of new skill.";
        }

        private void ApplyZeroStats()
        {
            TikTokText = "TikTok: Clean! ✨";
            InstagramText = "Instagram: Clean! ✨";
            YouTubeText = "YouTube: Clean! ✨";
            TotalTimeText = "For a whopping total of: 0 hours!";
            BooksText = "You could have read 0.0 books.";
            MoneyText = "You lost out on 0.00 zł (at 31.4/hr).";
            GymText = "You missed 0.0 gym sessions.";
            SkillText = "You could learn 0.0000% of new skill.";
        }

        private static string FormatAppText(string appName, int minutes)
        {
            if (minutes <= 0)
            {
                return $"{appName}: Clean! ✨";
            }

            string hoursSuffix = minutes > 60 ? $" ({Math.Round(minutes / 60.0, 1)} hrs)" : string.Empty;
            return $"{appName}: {minutes} mins{hoursSuffix}";
        }
    }
}
