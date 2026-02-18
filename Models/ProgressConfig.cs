namespace PvPmo.Models
{
    [Table("ProgressConfig")]
    public class ProgressConfig
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(100)]
        public string? TabellaSql { get; set; }

        [MaxLength(100)]
        public string? DbName { get; set; }

        [MaxLength(500)]
        public string? Descrizione { get; set; }
    }
}
