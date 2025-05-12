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
            Azione = reader["Azione"]?.ToString();
            ColDaMod = reader["ColDaMod"]?.ToString();
            Modifica = reader["Modifica"]?.ToString();
            TipoCol = reader["TipoCol"]?.ToString();
            TabellaMod = reader["TabellaMod"]?.ToString();
            DbDest = reader["DbDest"]?.ToString();
            InpType = reader["InpType"]?.ToString();
        }
    }
}
