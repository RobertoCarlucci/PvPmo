namespace PvPmo.Models
{
    public class CaricaTabOrigini : ICaricabileDaDataReader
    {
        public string? DbInp { get; set; }
        public string? Tabella { get; set; }
        public string? DbDest { get; set; }
        public string? TabellaSql { get; set; }
        public string? WorkSheet { get; set; }
        public string? InpType { get; set; }

        public void FromReader(MySqlDataReader reader)
        {
            DbInp = reader["DbInp"]?.ToString();
            Tabella = reader["Tabella"]?.ToString();
            DbDest = reader["DbDest"]?.ToString();
            TabellaSql = reader["TabellaSql"]?.ToString();
            WorkSheet = reader["WorkSheet"]?.ToString();
            InpType = reader["InpType"]?.ToString();
        }
    }

}
