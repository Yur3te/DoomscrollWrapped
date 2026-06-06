using System;
using Microsoft.Maui.Controls;

using DoomscrollWrapped.Services;

namespace DoomscrollWrapped.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly Supabase.Client _supabaseClient;

        public LoginPage(Supabase.Client supabaseClient)
        {
            InitializeComponent();
            _supabaseClient = supabaseClient;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert("Error", "Please enter both email and password.", "OK");
                return;
            }

            try
            {
                IsBusy = true;
                LoginBtn.IsEnabled = false;

                var session = await _supabaseClient.Auth.SignIn(EmailEntry.Text.Trim(), PasswordEntry.Text);

                if (session != null && session.User != null)
                {
                    new SupabaseSessionHandler().SaveSession(session);

                    var window = this.Window;
                    if (window != null)
                    {
                        window.Page = new AppShell(_supabaseClient);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Login Failed", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
                LoginBtn.IsEnabled = true;
            }
        }

        private async void OnSignUpNavigateClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage(_supabaseClient));
        }
    }
}
