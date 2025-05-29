namespace PvPmo.Models
{
    public class CaricaTabData : ICaricabileDaDataReader
    {
        public DateOnly? PV_TotalData { get; set; }
        public DateOnly? GlobalTimesheetExtractData { get; set; }
        public DateOnly? PV_MeseSuAnno { get; set; }
        public void FromReader(MySqlDataReader reader)
        {
            PV_TotalData = reader["PV_TotalData"] as DateOnly?;
            GlobalTimesheetExtractData = reader["GlobalTimesheetExtractData"] as DateOnly?;
            PV_MeseSuAnno = reader["PV_MeseSuAnno"] as DateOnly?;
        }
    }    
}
