namespace PvPmo.Models
{
    public class CaricaTabNorm : ICaricabileDaDataReader
    {
        public string? Azione { get; set; }
        public string? ColDaMod { get; set; }
        public string? Modifica { get; set; }
        public string? TipoCol { get; set; }
        public string? TabellaMod { get; set; }
        public string? DbDest { get; set; }
        public string? InpType { get; set; }

        public void FromReader(MySqlDataReader reader)
        {
            Azione = reader["azione"]?.ToString();
            ColDaMod = reader["coldamod"]?.ToString();
            Modifica = reader["modifica"]?.ToString();
            TipoCol = reader["tipocol"]?.ToString();
            TabellaMod = reader["tabellamod"]?.ToString();
            DbDest = reader["dbdest"]?.ToString();
            InpType = reader["inptype"]?.ToString();
        }
    }
}
