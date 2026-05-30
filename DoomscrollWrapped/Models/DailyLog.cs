using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace DoomscrollWrapped.Models
{
    [Table("daily_logs")]
    public class DailyLog : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("log_date")]
        public string LogDate { get; set; } = string.Empty;

        [Column("app_name")]
        public string AppName { get; set; } = string.Empty;

        [Column("wasted_minutes")]
        public int WastedMinutes { get; set; }
    }
}