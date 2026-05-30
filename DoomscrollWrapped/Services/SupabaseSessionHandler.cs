using Microsoft.Maui.Storage;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using System.Threading.Tasks;

namespace DoomscrollWrapped.Services
{
    public class SupabaseSessionHandler : IGotrueSessionPersistence<Session>
    {
        private const string SESSION_KEY = "SupabaseSession";

        public void SaveSession(Session session)
        {
            if (session != null)
            {
                try
                {
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(session);
                    Preferences.Set(SESSION_KEY, json);
                    System.Diagnostics.Debug.WriteLine("[SupabaseSession] Session saved successfully.");
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[SupabaseSession] Save failed: {ex.Message}");
                }
            }
        }

        public void DestroySession()
        {
            Preferences.Remove(SESSION_KEY);
        }

        public Session? LoadSession()
        {
            try
            {
                var json = Preferences.Get(SESSION_KEY, string.Empty);
                if (!string.IsNullOrEmpty(json))
                {
                    var session = Newtonsoft.Json.JsonConvert.DeserializeObject<Session>(json);
                    System.Diagnostics.Debug.WriteLine("[SupabaseSession] Session loaded successfully.");
                    return session;
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SupabaseSession] Load failed: {ex.Message}");
                // If it fails to deserialize due to a schema change, nuke it
                DestroySession();
            }
            return null;
        }
    }
}