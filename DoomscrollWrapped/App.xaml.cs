using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

using DoomscrollWrapped.Views;
using DoomscrollWrapped.Services;

namespace DoomscrollWrapped
{
    public partial class App : Application
    {
        private readonly Supabase.Client? _supabaseClient;

        public App(Supabase.Client? supabaseClient = null)
        {
            InitializeComponent();
            _supabaseClient = supabaseClient;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window();

            window.Page = new ContentPage { BackgroundColor = Color.FromArgb("#121212") };

            return window;
        }

        protected override async void OnStart()
        {
            base.OnStart();

            if (_supabaseClient != null)
            {
                try
                {
                    string rawJson = Preferences.Default.Get("SupabaseSession", "NONE");
                    bool hasJson = rawJson != "NONE" && rawJson.Length > 10;

                    if (hasJson)
                    {
                        var restoredSession = Newtonsoft.Json.JsonConvert.DeserializeObject<Supabase.Gotrue.Session>(rawJson);
                        if (restoredSession != null)
                        {
                            await _supabaseClient.Auth.SetSession(restoredSession.AccessToken, restoredSession.RefreshToken);
                        }
                    }
                    else
                    {
                        await _supabaseClient.InitializeAsync();
                    }

                    if (_supabaseClient.Auth.CurrentSession != null)
                    {
                        Application.Current!.Windows[0].Page = new AppShell(_supabaseClient);
                    }
                    else
                    {
                        Application.Current!.Windows[0].Page = new NavigationPage(new LoginPage(_supabaseClient));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"BOOT CRASH: {ex.Message}");
                    Application.Current!.Windows[0].Page = new NavigationPage(new LoginPage(_supabaseClient));
                }
            }
        }
    }
}