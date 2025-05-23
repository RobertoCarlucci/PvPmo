using PvPmo.View;

namespace PvPmo.UpdateDb
{
    public partial class TestDateImpExl
    {
        // Test delle tabelle di Uptd prima di procedere all'aggiornamento
        // del Db di produzione.

        public static async Task<bool> FinalizzaUptd()
        {
            //    // Carico il File dall'archivio con la sequenza da svolgere
            //    // e provvedo all'esecuzione.

            var repo = new CaricaTabRepository<CaricaTabFinalizza>("pvpmo_origine", "finalizza");
            var dati = await repo.GetAllAsync();

            bool tuttoOk = true;

            // Se non ci sono tabelle da testare esco.
            // Eseguo i test sui file exl di update importati in pmo_origine.

            foreach (var n in dati)
            {
                bool test = await TestFileExlImp(n.Azione, n.TabConfronto, n.ColConfronto, n.TabTestare,
                                                 n.ColDaTestare, n.DbTabConfronto, n.DbTabTest);

                if (!test) tuttoOk = false;
            }
            tuttoOk = await FinalizzaUpdateProdDb.ApplicaUptd("UPTD");
            return tuttoOk;
        }

        // Eseguo i test sui file exl di update importati in pmo_origine
        public static async Task<bool> TestFileExlImp(string azione, string tabProd, 
            string colProd, string tabUptd, string colUptd, string dbProd, string dbUptd)
        {
            bool tuttoOk = true;

            // Produzione corrisponde alla tabella di Uptd.
            // La tabella di produzione è quella di confronto.
            // La tabella di uptd è quella da testare.

            DataTable dataProd = new();
            DataTable dataUptd = new();

            string connProd = Conn.MysqlConn(dbProd);
            string connUptd = Conn.MysqlConn(dbUptd);
                        
            switch (azione)
            {
                //Test Valore Data e Record Data presenti in Uptd
                // Confrontandolo con la tabella 01_tabella_data del Db di produzione.

                case "TST":
                    string qProd = $"SELECT `{colProd}` FROM `{tabProd}` ORDER BY `{colProd}` ASC;";
                    string qUptd = $"SELECT DISTINCT `{colUptd}` FROM `{tabUptd}` ORDER BY `{colUptd}` ASC;";

                    await SqlAsync.SqlQryDataTable(connProd, qProd, dataProd, 60);
                    await SqlAsync.SqlQryDataTable(connUptd, qUptd, dataUptd, 60);

                    // Rimuovo righe nulle da produzione.
                    // Se non lo faccio mi da errore di confronto.

                    dataProd = dataProd.AsEnumerable()
                        .Where(r => !string.IsNullOrWhiteSpace(r[colProd]?.ToString()))
                        .CopyToDataTable();

                    int countProd = dataProd.Rows.Count;
                    int countUptd = dataUptd.Rows.Count;

                    if (countUptd != countProd)
                    {
                        await Shell.Current.DisplayAlert("Test righe data", "Numero righe diverso tra produzione e uptd.", "OK");
                        await Shell.Current.GoToAsync("//MainPage");
                        tuttoOk = false;
                    }

                    // Viene testato il valore delle date se corrispondenti.
                    // Confronto i valori delle date tra produzione e uptd.

                    var valoriProd = dataProd.AsEnumerable().Select(r => r[colProd]?.ToString()).ToHashSet();
                    var valoriUptd = dataUptd.AsEnumerable().Select(r => r[colUptd]?.ToString()).ToHashSet();

                    if (!valoriProd.SetEquals(valoriUptd))
                    {
                        bool correzione = await Shell.Current.DisplayAlert(
                            "Date non corrispondenti",
                            "I valori delle date tra produzione e aggiornamento non corrispondono.\nVuoi correggerli manualmente?",
                            "Sì", "No");

                        if (!correzione)
                        {
                            await Shell.Current.GoToAsync("//MainPage");
                            tuttoOk = false;
                        }
                        else
                        {
                            var vm = ServiceHelper.GetService<ModDataViewModel>();                                
                            await Shell.Current.GoToAsync(nameof(ModData));
                            tuttoOk = false; // fermiamo il test corrente, verrà rieseguito dopo la modifica
                        }
                    }
                    break;

                case "NUM":

                    // Viene testato il numero delle colonne presenti in Uptd rispetto alle
                    // tabelle presenti nel Db di produzione.
                        
                    string showProd = $"SHOW COLUMNS FROM `{tabProd}`;";
                    string showUptd = $"SHOW COLUMNS FROM `{tabUptd}`;";

                    await SqlAsync.SqlQryDataTable(connProd, showProd, dataProd, 30);
                    await SqlAsync.SqlQryDataTable(connUptd, showUptd, dataUptd, 30);

                    if (dataUptd.Rows.Count != dataProd.Rows.Count)
                    {
                        await Shell.Current.DisplayAlert("Test colonne",
                            $"Numero colonne diverso tra produzione e uptd tabella: ({tabProd}).", "OK");
                        tuttoOk = false;
                    }
                    break;                        
            }
            return tuttoOk;            
        }        
    }
}
