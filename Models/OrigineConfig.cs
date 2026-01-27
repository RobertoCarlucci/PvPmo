using SQLite;

namespace PvPmo.Models
{
    [Table("OrigineConfig")]
    public class OrigineConfig
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(100)]
        public string? DbInp { get; set; }

        [MaxLength(100)]
        public string? Tabella { get; set; }

        [MaxLength(100)]
        public string? DbDest { get; set; }

        [MaxLength(100)]
        public string? TabellaSql { get; set; }

        [MaxLength(100)]
        public string? WorkSheet { get; set; }

        [MaxLength(50)]
        public string? InpType { get; set; }
    }
}
