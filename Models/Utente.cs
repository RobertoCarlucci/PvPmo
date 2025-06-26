namespace PvPmo.Models
{
    public enum TipoUtente
    {
        Locale,
        Windows
    }

    public class Utente
    {
        public string NomeUtente { get; set; } = "";
        public string? PasswordHash { get; set; }
        public string Ruolo { get; set; } = "Operatore";
        public TipoUtente Tipo { get; set; }
    }

}
