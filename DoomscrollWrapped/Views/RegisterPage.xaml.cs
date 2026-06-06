using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Supabase.Gotrue;

namespace DoomscrollWrapped.Views
{
    public partial class RegisterPage : ContentPage
    {
        private readonly Supabase.Client _supabaseClient;

        public RegisterPage(Supabase.Client supabaseClient)
        {
            InitializeComponent();
            _supabaseClient = supabaseClient;
        }

        private async void OnCreateAccountClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text)
                || string.IsNullOrWhiteSpace(txtEmail.Text)
                || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                await DisplayAlert("Error", "Please enter a username, email, and password.", "OK");
                return;
            }

            try
            {
                IsBusy = true;
                CreateAccountBtn.IsEnabled = false;

                var metadata = new Dictionary<string, object> { { "username", txtUsername.Text.Trim() } };
                var options = new SignUpOptions { Data = metadata };

                var signUpSession = await _supabaseClient.Auth.SignUp(txtEmail.Text.Trim(), txtPassword.Text, options);

                if (signUpSession?.User != null)
                {
                    await DisplayAlert("Success", "Account created! You can now log in.", "OK");
                    await Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Sign Up Failed", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
                CreateAccountBtn.IsEnabled = true;
            }
        }

        private async void OnLogInClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
