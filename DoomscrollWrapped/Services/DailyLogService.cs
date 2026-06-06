using DoomscrollWrapped.Models;
using Supabase;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoomscrollWrapped.Services
{
    public class DailyLogService : IDailyLogService
    {
        private readonly Client _supabaseClient;

        public DailyLogService(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<IReadOnlyDictionary<string, int>> GetAllTimeMinutesByAppAsync()
        {
            string? userId = _supabaseClient.Auth.CurrentUser?.Id
                ?? _supabaseClient.Auth.CurrentSession?.User?.Id;

            if (string.IsNullOrEmpty(userId))
            {
                return new Dictionary<string, int>();
            }

            var response = await _supabaseClient
                .From<DailyLog>()
                .Where(x => x.UserId == userId)
                .Get();

            var totals = new Dictionary<string, int>();
            foreach (var log in response.Models)
            {
                if (string.IsNullOrEmpty(log.AppName))
                {
                    continue;
                }

                if (totals.TryGetValue(log.AppName, out int existing))
                {
                    totals[log.AppName] = existing + log.WastedMinutes;
                }
                else
                {
                    totals[log.AppName] = log.WastedMinutes;
                }
            }

            return totals;
        }

        public async Task<IReadOnlyList<LeaderboardEntry>> GetGlobalLeaderboardAsync()
        {
            var logsResponse = await _supabaseClient
                .From<DailyLog>()
                .Get();

            var totalsByUser = new Dictionary<string, int>();
            foreach (var log in logsResponse.Models)
            {
                if (string.IsNullOrEmpty(log.UserId))
                {
                    continue;
                }

                if (totalsByUser.TryGetValue(log.UserId, out int existing))
                {
                    totalsByUser[log.UserId] = existing + log.WastedMinutes;
                }
                else
                {
                    totalsByUser[log.UserId] = log.WastedMinutes;
                }
            }

            var nicknamesByUserId = new Dictionary<string, string>();
            try
            {
                var profilesResponse = await _supabaseClient
                    .From<Profile>()
                    .Get();

                foreach (var profile in profilesResponse.Models)
                {
                    if (!string.IsNullOrWhiteSpace(profile.Username))
                    {
                        nicknamesByUserId[profile.Id] = profile.Username;
                    }
                }
            }
            catch
            {
                
            }

            var entries = totalsByUser
                .Select(kvp => new LeaderboardEntry
                {
                    Nickname = ResolveNickname(kvp.Key, nicknamesByUserId),
                    TotalMinutes = kvp.Value
                })
                .OrderByDescending(entry => entry.TotalMinutes)
                .ThenBy(entry => entry.Nickname)
                .ToList();

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Rank = i + 1;
            }

            return entries;
        }

        private static string ResolveNickname(string userId, IReadOnlyDictionary<string, string> nicknamesByUserId)
        {
            if (nicknamesByUserId.TryGetValue(userId, out string? nickname) && !string.IsNullOrWhiteSpace(nickname))
            {
                return nickname;
            }

            return userId.Length > 8 ? $"User {userId[..8]}" : $"User {userId}";
        }
    }
}
