using SQLite;

namespace PvPmo.Models;

public enum TipoUtente
{
    Locale,
    Windows
}

[Table("Utenti")]
public class Utente
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed, MaxLength(100)]
    public string NomeUtente { get; set; } = "";

    [MaxLength(64)]
    public string? PasswordHash { get; set; }

    [MaxLength(50)]
    public string Ruolo { get; set; } = "Operatore";

    public TipoUtente Tipo { get; set; }
}