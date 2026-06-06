using DoomscrollWrapped.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DoomscrollWrapped.Services
{
    public interface IDailyLogService
    {
        Task<IReadOnlyDictionary<string, int>> GetAllTimeMinutesByAppAsync();

        Task<IReadOnlyList<LeaderboardEntry>> GetGlobalLeaderboardAsync();
    }
}
