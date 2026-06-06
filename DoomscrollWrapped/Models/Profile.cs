using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace DoomscrollWrapped.Models
{
    [Table("profiles")]
    public class Profile : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = string.Empty;

        [Column("username")]
        public string Username { get; set; } = string.Empty;
    }
}
