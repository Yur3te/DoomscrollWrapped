using Microsoft.Maui.Controls;
using System;

using DoomscrollWrapped.Views;
using DoomscrollWrapped.Services;

namespace DoomscrollWrapped
{
    public partial class AppShell : Shell
    {
        private readonly Supabase.Client _supabaseClient;
        public string? UserEmail { get; set; }

        public AppShell(Supabase.Client supabaseClient)
        {
            InitializeComponent();
            _supabaseClient = supabaseClient;

            UserEmail = _supabaseClient.Auth.CurrentUser?.Email;
            BindingContext = this;
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            try
            {
                await _supabaseClient.Auth.SignOut();
            }
            catch (Exception)
            {

            }

            Preferences.Default.Remove("SupabaseSession");
            new SupabaseSessionHandler().DestroySession();

            Application.Current!.Windows[0].Page = new NavigationPage(new LoginPage(_supabaseClient));
        }

        private async void OnMenuTodayTapped(object sender, EventArgs e)
        {
            Current.FlyoutIsPresented = false;
            await Current.GoToAsync("//MainPage");
        }

        private async void OnWastedOnScrollTapped(object sender, EventArgs e)
        {
            Current.FlyoutIsPresented = false;
            await Current.GoToAsync("//WastedOnScrollPage");
        }

        private async void OnMenuLeaderboardTapped(object sender, EventArgs e)
        {
            Current.FlyoutIsPresented = false;
            await Current.GoToAsync("//LeaderboardPage");
        }
    }
}
