using DoomscrollWrapped.Models;
using Supabase;
using System;
using System.Threading.Tasks;

namespace DoomscrollWrapped.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly Supabase.Client? _supabaseClient;

        public MainPage(Supabase.Client? supabaseClient = null)
        {
            InitializeComponent();
            _supabaseClient = supabaseClient;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadAndSyncDataAsync();
        }

        private async Task LoadAndSyncDataAsync()
        {
            if (_supabaseClient == null)
            {
                await DisplayAlert("Error", "Supabase client is not initialized.", "OK");
                LoadingSpinner.IsRunning = false;
                LoadingSpinner.IsVisible = false;
                return;
            }

            int totalWastedMinutes = 0;
            int tiktokMinutes = 0;
            int instagramMinutes = 0;
            int youtubeMinutes = 0;

#if ANDROID
            try
            {
                var context = Android.App.Application.Context;
                var usageStatsManager = (Android.App.Usage.UsageStatsManager?)context.GetSystemService(Android.Content.Context.UsageStatsService);

                if (usageStatsManager != null)
                {
                    DateTime localMidnight = DateTime.Today;
                    long startTime = new DateTimeOffset(localMidnight).ToUnixTimeMilliseconds();
                    long endTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    var events = usageStatsManager.QueryEvents(startTime, endTime);

                    if (events != null)
                    {
                        var targetApps = new System.Collections.Generic.Dictionary<string, string>
                        {
                            { "com.zhiliaoapp.musically", "TikTok" },
                            { "com.instagram.android", "Instagram" },
                            { "com.google.android.youtube", "YouTube" }
                        };

                        var appUsageMinutes = new System.Collections.Generic.Dictionary<string, int>();
                        var appStartTimes = new System.Collections.Generic.Dictionary<string, long>();
                        var appMs = new System.Collections.Generic.Dictionary<string, long>();

                        var eventOut = new Android.App.Usage.UsageEvents.Event();
                        while (events.HasNextEvent)
                        {
                            events.GetNextEvent(eventOut);
                            if (eventOut.PackageName != null && targetApps.ContainsKey(eventOut.PackageName))
                            {
                                string appName = targetApps[eventOut.PackageName];

                                if (eventOut.EventType == Android.App.Usage.UsageEventType.ActivityResumed)
                                {
                                    appStartTimes[appName] = eventOut.TimeStamp;
                                }
                                else if (eventOut.EventType == Android.App.Usage.UsageEventType.ActivityPaused)
                                {
                                    if (appStartTimes.TryGetValue(appName, out long startTimestamp))
                                    {
                                        long elapsedMs = eventOut.TimeStamp - startTimestamp;
                                        if (elapsedMs > 0)
                                        {
                                            if (appMs.ContainsKey(appName))
                                                appMs[appName] += elapsedMs;
                                            else
                                                appMs[appName] = elapsedMs;
                                        }
                                        appStartTimes.Remove(appName);
                                    }
                                }
                            }
                        }

                        foreach (var kvp in appMs)
                        {
                            int minutes = (int)(kvp.Value / 60000);
                            if (minutes > 0)
                            {
                                appUsageMinutes[kvp.Key] = minutes;
                            }
                        }

                        if (appUsageMinutes.Count == 0)
                        {
                            await DisplayAlert("Notice", "No usage recorded for today or you haven't opened matched apps.", "OK");
                            LoadingSpinner.IsRunning = false;
                            LoadingSpinner.IsVisible = false;
                            return;
                        }

                        string todayStr = DateTime.Today.ToString("yyyy-MM-dd");
                        string? userId = _supabaseClient.Auth.CurrentUser?.Id;

                        foreach (var kvp in appUsageMinutes)
                        {
                            totalWastedMinutes += kvp.Value;

                            if (kvp.Key == "TikTok") tiktokMinutes = kvp.Value;
                            else if (kvp.Key == "Instagram") instagramMinutes = kvp.Value;
                            else if (kvp.Key == "YouTube") youtubeMinutes = kvp.Value;

                            var log = new DailyLog
                            {
                                UserId = userId,
                                LogDate = todayStr,
                                AppName = kvp.Key,
                                WastedMinutes = kvp.Value
                            };

                            DailyLog? existingLog = null;
                            try
                            {
                                existingLog = await _supabaseClient.From<DailyLog>()
                                    .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId)
                                    .Filter("log_date", Supabase.Postgrest.Constants.Operator.Equals, todayStr)
                                    .Filter("app_name", Supabase.Postgrest.Constants.Operator.Equals, kvp.Key)
                                    .Single();
                            }
                            catch (Exception)
                            {
                                
                            }

                            if (existingLog != null)
                            {
                                log.Id = existingLog.Id;
                                await _supabaseClient.From<DailyLog>().Update(log);
                            }
                            else
                            {
                                await _supabaseClient.From<DailyLog>().Insert(log);
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert("Notice", "No usage returned. Did you grant 'Usage Access' permission in Android Settings?", "OK");
                        LoadingSpinner.IsRunning = false;
                        LoadingSpinner.IsVisible = false;
                        return;
                    }
                }
                else
                {
                    await DisplayAlert("Error", "UsageStatsManager is null here.", "OK");
                    LoadingSpinner.IsRunning = false;
                    LoadingSpinner.IsVisible = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"USAGE STATS OR SUPABASE CRASH: {ex.Message}");
                await DisplayAlert("Error", $"Could not process data: {ex.Message}", "OK");
                LoadingSpinner.IsRunning = false;
                LoadingSpinner.IsVisible = false;
                return;
            }
#else
            await DisplayAlert("Not Supported", "Automated tracking is only supported on Android right now.", "OK");
            LoadingSpinner.IsRunning = false;
            LoadingSpinner.IsVisible = false;
            return;
#endif

            if (totalWastedMinutes > 0)
            {
                double hours = totalWastedMinutes / 60.0;

                TikTokLabel.Text = tiktokMinutes > 0 ? $"TikTok: {tiktokMinutes} mins {(tiktokMinutes > 60 ? $"({Math.Round(tiktokMinutes/60.0, 1)} hrs)" : "")}" : "TikTok: Clean! ✨";
                InstagramLabel.Text = instagramMinutes > 0 ? $"Instagram: {instagramMinutes} mins {(instagramMinutes > 60 ? $"({Math.Round(instagramMinutes/60.0, 1)} hrs)" : "")}" : "Instagram: Clean! ✨";
                YouTubeLabel.Text = youtubeMinutes > 0 ? $"YouTube: {youtubeMinutes} mins {(youtubeMinutes > 60 ? $"({Math.Round(youtubeMinutes/60.0, 1)} hrs)" : "")}" : "YouTube: Clean! ✨";

                double booksWasted = hours / 5.0;
                double moneyWasted = hours * 31.4;
                double gymSessions = hours / 1.5;
                double percentOfnewSkill = (hours / 20.0) * 100.0;

                BooksLabel.Text = $"You could have read {booksWasted:F1} books.";
                MoneyLabel.Text = $"You lost out on {moneyWasted:F2} zł (at 31.4/hr).";
                GymLabel.Text = $"You missed {gymSessions:F1} gym sessions.";
                SkillLabel.Text = $"You could learn {percentOfnewSkill:F4}% of new skill.";

                LoadingSpinner.IsRunning = false;
                LoadingSpinner.IsVisible = false;
                ResultsGrid.IsVisible = true;

                SemanticScreenReader.Announce("Displaying today's wasted stats.");
            }
            else
            {
                LoadingSpinner.IsRunning = false;
                LoadingSpinner.IsVisible = false;
                await DisplayAlert("Congratulations!", "You haven't wasted any time on targeted apps today!", "OK");
            }
        }
    }
}
