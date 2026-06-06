using Microsoft.Extensions.Logging;
using Supabase;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

using DoomscrollWrapped.Views;
using DoomscrollWrapped.Services;
using DoomscrollWrapped.ViewModels;

namespace DoomscrollWrapped
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            string supabaseUrl = "";
            string supabaseKey = "";

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream("DoomscrollWrapped.secrets.json");

                if (stream == null)
                {
                    throw new FileNotFoundException("Missing secrets.json. Please create it with your Supabase credentials to run the app.");
                }

                using var reader = new StreamReader(stream);
                var jsonContent = reader.ReadToEnd();

                using var doc = JsonDocument.Parse(jsonContent);
                var root = doc.RootElement;

                supabaseUrl = root.GetProperty("SupabaseUrl").GetString() ?? "";
                supabaseKey = root.GetProperty("SupabaseKey").GetString() ?? "";

                if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
                {
                    throw new InvalidDataException("secrets.json is missing 'SupabaseUrl' or 'SupabaseKey'.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"SECRETS CONFIGURATION ERROR: {ex.Message}", ex);
            }

            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true,
                SessionHandler = new SupabaseSessionHandler()
            };

            try
            {
                var supabaseClient = new Supabase.Client(supabaseUrl, supabaseKey, options);
                builder.Services.AddSingleton(supabaseClient);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SUPABASE INIT CRASH: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"INNER EXCEPTION: {ex.InnerException.Message}");
                }
            }

            builder.Services.AddSingleton<IDailyLogService, DailyLogService>();
            builder.Services.AddTransient<WastedOnScrollViewModel>();
            builder.Services.AddTransient<LeaderboardViewModel>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<WastedOnScrollPage>();
            builder.Services.AddTransient<LeaderboardPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<App>();

            return builder.Build();
        }
    }
}
