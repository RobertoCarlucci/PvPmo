namespace PvPmo.Models
{
    public enum JsonConfigType
    {
        Origine,
        Normalizza,
        Progress,
        Finalizza,
        Utenti
    }

    public class ConfigOption
    {
        public JsonConfigType Tipo { get; set; }

        public string TitoloConfig => Tipo.ToString(); // oppure una versione custom

        // Se vuoi una descrizione estesa:
        public string Descrizione =>
           Tipo switch
           {
               JsonConfigType.Finalizza => "Configurazione finale delle trasformazioni",
               JsonConfigType.Normalizza => "Regole di normalizzazione colonne",
               JsonConfigType.Origine => "Origine dei dati di input",
               JsonConfigType.Progress => "Monitoraggio avanzamento attività",
               JsonConfigType.Utenti => "Utenti e permessi",
               _ => "Altro tipo di configurazione"
           };
    }
}
