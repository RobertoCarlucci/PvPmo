namespace PvPmo.Models
{
    [Table("FileSetupConfig")]
    public class FileSetupConfig
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(100)]
        public string? FileImp { get; set; }

        [MaxLength(100)]
        public string? TabDest { get; set; }

        [MaxLength(500)]
        public string? Descrizione { get; set; }
    }
}
