namespace PvPmo.Models
{
    public class CaricaProgressDescrizione : ICaricabileDaDataReader
    {
        public string? TabellaSql { get; set; }
        public string? DbName { get; set; }
        public string? Descrizione { get; set; }        
        public void FromReader(MySqlDataReader reader)
        {
            TabellaSql = reader["TabellaSql"]?.ToString();
            DbName = reader["DbName"]?.ToString();
            Descrizione = reader["Descrizione"]?.ToString();            
        }
    }    
}
