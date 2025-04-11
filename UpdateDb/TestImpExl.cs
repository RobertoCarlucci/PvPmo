using PvPmo.Service;

namespace PvPmo.UpdateDb
{
    public partial class TestDateImpExl
    {
        // Test delle tabelle di Uptd prima di procedere all'importazione.
        public static async Task<bool> FinalizzaUptd()
        {
            // Carico il File dall'archivio con la sequenza da svolgere
            // e provvedo all'esecuzione.
            CaricaTabFinalizzaService _finalizza = new CaricaTabFinalizzaService();
            bool Bol = false;

            foreach (var n in _finalizza.caricaTabFin) 
            {
                string? azione = n.Azione.ToString();
                string? tabConf = n.TabConfronto.ToString();
                string? colConf = n.ColConfronto.ToString();
                string? tabTest = n.TabTestare.ToString();
                string? colTest = n.ColDaTestare.ToString();
                string? dbConf = n.DbTabConfronto.ToString();
                string? dbTest = n.DbTabTest.ToString();
                Bol = await TestFileExlImp(azione, tabConf, colConf, tabTest, colTest,dbConf, dbTest);
            }
            return Bol;
        }
        // Eseguo i test sui file exl di update.
        public static async Task<bool> TestFileExlImp(
            string _azione, string _tabConf, string _colConf, string _tabTest, string _colTest, string _dbConf, string _dbTest)
        {
            // Test corrisponde alla tabella Uptd.
            DataTable dataTest = new DataTable();
            DataTable dataConf = new DataTable();
            bool Bol = false;
            string StrConnConf = Conn.MysqlConn(_dbConf);
            string StrConnTest = Conn.MysqlConn(_dbTest);

            switch (_azione)
            {
                //Test Valore Data e Record Data presenti in Uptd
                case "TST":
                    // Viene effettuato il test delle date presenti in Uptd.
                    string QrySqlTest = "SELECT DISTINCT " + _colTest + " FROM " + _tabTest + " ORDER BY " + _colTest + " ASC;";
                    string QrySqlConf = "SELECT " + _colConf + " FROM " + _tabConf + " ORDER BY " + _colConf + " ASC;";

                    dataTest = await SqlAsync.SqlQryDataTable(StrConnTest, QrySqlTest, dataTest, 60);
                    dataConf = await SqlAsync.SqlQryDataTable(StrConnConf, QrySqlConf, dataConf, 60);
                    DataRow[] temp = dataConf.Select();
                    foreach (DataRow row in temp)
                    {
                        string? res = row[_colConf].ToString();
                        if (string.IsNullOrEmpty(res))
                        {
                            dataConf.Rows.Remove(row);
                        }
                    }
                    var countTest = dataTest.Rows.Count;
                    var countConf = dataConf.Rows.Count;
                    if (countTest != countConf) { Bol = true; }
                    
                    // Viene testato il valore delle date
                    var result = dataConf.AsEnumerable().Intersect(dataTest.AsEnumerable(), DataRowComparer.Default);
                    if (result == null) { Bol = true; }

                    break;
                case "NUM":
                    // Viene testato il numero delle colonne presenti in Uptd.
                    QrySqlTest = "SELECT * FROM `information_schema`.`COLUMNS` WHERE TABLE_SCHEMA = '"
                        + _dbTest + "' AND TABLE_NAME = '" + _tabTest + "' ORDER BY ORDINAL_POSITION;";
                    QrySqlConf = "SELECT * FROM `information_schema`.`COLUMNS` WHERE TABLE_SCHEMA = '"
                        + _dbConf + "' AND TABLE_NAME = '" + _tabConf + "' ORDER BY ORDINAL_POSITION;";
                    
                    await SqlAsync.SqlQryDataReader(StrConnTest, QrySqlTest, dataTest, 30);
                    await SqlAsync.SqlQryDataReader(StrConnConf, QrySqlConf, dataConf, 30);

                    int _dif = dataTest.Rows.Count - dataConf.Rows.Count;
                    if (_dif != 0) { return true; }

                    break;
                case "REN":

                    break;
            }
            
            
            return Bol;       
        }
    }
}
