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
                string? tabProd = n.TabConfronto.ToString();
                string? colProd = n.ColConfronto.ToString();
                string? tabUptd = n.TabTestare.ToString();
                string? colUptd = n.ColDaTestare.ToString();
                string? dbProd = n.DbTabConfronto.ToString();
                string? dbUptd = n.DbTabTest.ToString();
                Bol = await TestFileExlImp(azione, tabProd, colProd, tabUptd, colUptd, dbProd, dbUptd);
            }
            return Bol;
        }
        // Eseguo i test sui file exl di update.
        public static async Task<bool> TestFileExlImp(
            string _azione, string _tabProd, string _colProd, string _tabUptd, string _colUptd, string _dbProd, string _dbUptd)
        {
            // Test corrisponde alla tabella Uptd.
            DataTable dataProd = new DataTable();
            DataTable dataUptd = new DataTable();
            
            bool Bol = false;
            string StrConnProd = Conn.MysqlConn(_dbProd);
            string StrConnUptd = Conn.MysqlConn(_dbUptd);

            switch (_azione)
            {
                //Test Valore Data e Record Data presenti in Uptd
                case "TST":
                    // Viene effettuato il test del numero di righe della data presenti in Uptd
                    // Confrontandolo con la tabella 01_tabella_data del Db di produzione.
                    string QrySqlProd = "SELECT " + _colProd + " FROM " + _tabProd + " ORDER BY " + _colProd + " ASC;";
                    string QrySqlUptd = "SELECT DISTINCT " + _colUptd + " FROM " + _tabUptd + " ORDER BY " + _colUptd + " ASC;";

                    dataProd = await SqlAsync.SqlQryDataTable(StrConnProd, QrySqlProd, dataProd, 60);
                    dataUptd = await SqlAsync.SqlQryDataTable(StrConnUptd, QrySqlUptd, dataUptd, 60);
                    DataRow[] temp = dataProd.Select();                    
                    
                    foreach (DataRow row in temp)
                    {
                        string? res = row[_colProd].ToString();
                        if (string.IsNullOrEmpty(res))
                        {
                            dataProd.Rows.Remove(row);
                        }
                    }
                    var countProd = dataProd.Rows.Count;
                    var countUptd = dataUptd.Rows.Count;                    
                    if (countUptd != countProd) 
                    {
                        await Shell.Current.DisplayAlert
                            ("Test numero righe  data !", $"Le  non corrispondono.", "Ok");
                        Bol = true; 
                    }
                    
                    // Viene testato il valore delle date se corrispondenti.
                    var result = dataProd.AsEnumerable().Intersect(dataUptd.AsEnumerable(), DataRowComparer.Default);
                    if (result != null) 
                    {
                        await Shell.Current.DisplayAlert
                            ("Test Valore date !", $"Le date non corrispondono.", "Ok");
                        Bol = true; 
                    }                    
                    break;
                case "NUM":
                    // Viene testato il numero delle colonne presenti in Uptd rispetto alle tabelle presenti
                    // nel Db di produzione.                    
                    QrySqlProd = "SHOW COLUMNS FROM '" + _tabProd + ";";
                    QrySqlUptd = "SHOW COLUMNS FROM '" + _tabUptd + ";";

                    await SqlAsync.SqlQryDataReader(StrConnUptd, QrySqlUptd, dataUptd, 30);
                    await SqlAsync.SqlQryDataReader(StrConnProd, QrySqlProd, dataProd, 30);

                    int _dif = dataUptd.Rows.Count - dataProd.Rows.Count;
                    if (_dif != 0) 
                    {
                        await Shell.Current.DisplayAlert
                            ("Test Numero colonne !", $"Il numero delle colonne non corrisponde.", "Ok");
                        return true; 
                    }                    
                    break;
                    // Vengono cambiati i nomi colonna adeguandoli a quelli delle tabelle presenti
                    // nel Db di produzione.
                case "REN":

                    break;
            }
            
            
            return Bol;       
        }
    }
}
