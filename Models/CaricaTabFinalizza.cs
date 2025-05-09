namespace PvPmo.Models
{
    public class CaricaTabFinalizza : ICaricabileDaDataReader
    {
        public string? Azione { get; set; }
        public string? TabConfronto { get; set; }
        public string? ColConfronto { get; set; }
        public string? TabTestare { get; set; }
        public string? ColDaTestare { get; set; }
        public string? DbTabConfronto { get; set; }
        public string? DbTabTest { get; set; }
        public void FromReader(MySqlDataReader reader)
        {
            Azione = reader["Azione"]?.ToString();
            TabConfronto = reader["TabConfronto"]?.ToString();
            ColConfronto = reader["ColConfronto"]?.ToString();
            TabTestare = reader["TabTestare"]?.ToString();
            ColDaTestare = reader["ColDaTestare"]?.ToString();
            DbTabConfronto = reader["DbTabConfronto"]?.ToString();
            DbTabTest = reader["DbTabTest"]?.ToString();
        }
    }
}
