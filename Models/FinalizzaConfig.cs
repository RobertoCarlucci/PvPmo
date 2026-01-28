using SQLite;

namespace PvPmo.Models;

[Table("FinalizzaConfig")]
public class FinalizzaConfig
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(100)]
    public string? Azione { get; set; }

    [MaxLength(100)]
    public string? TabConfronto { get; set; }

    [MaxLength(100)]
    public string? ColConfronto { get; set; }

    [MaxLength(100)]
    public string? TabTestare { get; set; }

    [MaxLength(100)]
    public string? ColDaTestare { get; set; }

    [MaxLength(100)]
    public string? DbTabConfronto { get; set; }

    [MaxLength(100)]
    public string? DbTabTest { get; set; }
}
