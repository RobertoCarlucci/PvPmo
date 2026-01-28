using SQLite;

namespace PvPmo.Models;

[Table("NormalizzaConfig")]
public class NormalizzaConfig
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(100)]
    public string? Azione { get; set; }

    [MaxLength(100)]
    public string? ColDaMod { get; set; }

    [MaxLength(500)]
    public string? Modifica { get; set; }

    [MaxLength(50)]
    public string? TipoCol { get; set; }

    [MaxLength(100)]
    public string? TabellaMod { get; set; }

    [MaxLength(100)]
    public string? DbDest { get; set; }

    [MaxLength(50)]
    public string? InpType { get; set; }
}


