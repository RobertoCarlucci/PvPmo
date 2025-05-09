namespace PvPmo.Models
{
    public class CaricaTabData : ICaricabileDaDataReader
    {
        public DateTime? PV_TotalData { get; set; }
        public DateTime? GlobalTimesheetExtractData { get; set; }
        public DateTime? PV_MeseSuAnno { get; set; }
        public void FromReader(MySqlDataReader reader)
        {
            PV_TotalData = reader["PV_TotalData"] as DateTime?;
            GlobalTimesheetExtractData = reader["GlobalTimesheetExtractData"] as DateTime?;
            PV_MeseSuAnno = reader["PV_MeseSuAnno"] as DateTime?;
        }
    }    
}
