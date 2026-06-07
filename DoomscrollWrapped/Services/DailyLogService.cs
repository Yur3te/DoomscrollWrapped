using DoomscrollWrapped.Models;
using Supabase;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoomscrollWrapped.Services
{
    public class DailyLogService : IDailyLogService
    {
        public const string UnknownAppName = "Unknown";
        private const int PageSize = 1000;
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

            var userLogs = await FetchUserDailyLogsAsync(userId);

            var totals = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var log in userLogs)
            {
                string appName = string.IsNullOrWhiteSpace(log.AppName)
                    ? UnknownAppName
                    : log.AppName.Trim();

                if (totals.TryGetValue(appName, out int existing))
                {
                    totals[appName] = existing + log.WastedMinutes;
                }
                else
                {
                    totals[appName] = log.WastedMinutes;
                }
            }

            return totals;
        }

        public async Task<IReadOnlyList<LeaderboardEntry>> GetGlobalLeaderboardAsync()
        {
            var allLogs = await FetchAllDailyLogsAsync();

            var totalsByUser = new Dictionary<string, int>();
            foreach (var log in allLogs)
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

        private async Task<List<DailyLog>> FetchUserDailyLogsAsync(string userId)
        {
            var userLogs = new List<DailyLog>();
            int offset = 0;

            while (true)
            {
                var response = await _supabaseClient
                    .From<DailyLog>()
                    .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId)
                    .Order(x => x.Id, Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Range(offset, offset + PageSize - 1)
                    .Get();

                if (response.Models.Count == 0)
                {
                    break;
                }

                userLogs.AddRange(response.Models);

                if (response.Models.Count < PageSize)
                {
                    break;
                }

                offset += PageSize;
            }

            return userLogs;
        }

        private async Task<List<DailyLog>> FetchAllDailyLogsAsync()
        {
            var allLogs = new List<DailyLog>();
            int offset = 0;

            while (true)
            {
                var response = await _supabaseClient
                    .From<DailyLog>()
                    .Order(x => x.Id, Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Range(offset, offset + PageSize - 1)
                    .Get();

                if (response.Models.Count == 0)
                {
                    break;
                }

                allLogs.AddRange(response.Models);

                if (response.Models.Count < PageSize)
                {
                    break;
                }

                offset += PageSize;
            }

            return allLogs;
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
