using PvPmo.View;

namespace PvPmo.Import
{
    public partial class TestDateImpExl
    {
        // Test delle tabelle di Uptd prima di procedere all'aggiornamento
        // del Db di produzione.

        public static async Task<bool> TestUptd(IProgress<string>? progress)
        {
            var descrizioni = await DescrizioniProgress.GetProgressDescriptionsAsync(await Conn.MysqlConn("pvpmo_origine"));

            //    // Carico il File dall'archivio con la sequenza da svolgere
            //    // e provvedo all'esecuzione.

            var repo = new CaricaTabRepository<CaricaTabFinalizza>("pvpmo_origine", "finalizza");
            var dati = await repo.GetAllAsync();           

            bool tuttoOk = true;

            // Se non ci sono tabelle da testare esco.
            // Eseguo i test sui file exl di update importati in pmo_origine.

            foreach (var n in dati)
            {
                var db = string.IsNullOrWhiteSpace(n.DbTabConfronto) ? "default" : n.DbTabConfronto;
                var key = $"{n.TabConfronto}|{db}";
                var label = descrizioni.TryGetValue(key, out var desc) ? desc : $"{n.TabConfronto} ({db})";
               
                switch (n.Azione)
                {
                    case "TST":
                        if (string.IsNullOrWhiteSpace(n.DbTabConfronto) || string.IsNullOrWhiteSpace(n.TabConfronto) ||
                            string.IsNullOrWhiteSpace(n.ColConfronto) || string.IsNullOrWhiteSpace(n.DbTabTest) ||
                            string.IsNullOrWhiteSpace(n.TabTestare) || string.IsNullOrWhiteSpace(n.ColDaTestare))
                        {
                            await Shell.Current.DisplayAlert("Errore", "Dati di test non completi.", "OK");
                            return false;
                        }
                        tuttoOk = await EseguiTST(n.DbTabConfronto, n.TabConfronto, n.DbTabTest,
                            n.TabTestare, label, n.ColConfronto, n.ColDaTestare, progress);
                        if (!tuttoOk) return false;
                        break;
                    case "NUM":
                        if (string.IsNullOrWhiteSpace(n.DbTabConfronto) || string.IsNullOrWhiteSpace(n.TabConfronto) ||
                            string.IsNullOrWhiteSpace(n.DbTabTest) || string.IsNullOrWhiteSpace(n.TabTestare))
                        {
                            await Shell.Current.DisplayAlert("Errore", "Dati di test non completi.", "OK");
                            return false;
                        }
                        tuttoOk = await TestNumCol(n.DbTabConfronto, n.TabConfronto, n.DbTabTest, n.TabTestare, label, progress);
                        if (!tuttoOk) return false;
                        break;
                    case "UPTD":
                        continue; // Non eseguo UPTD qui, ma alla fine di tutti i test.
                    case "CLN":
                        continue; // Non eseguo pulizia Key qui, ma solo a fine procedura.
                    default:
                        await Shell.Current.DisplayAlert("Azione sconosciuta", $"Azione {n.Azione} non riconosciuta.", "OK");
                        return !tuttoOk;
                }                           
            }            
            return tuttoOk;
        }

        public static async Task<bool> EseguiTST(string dbProd, string tabProd, string dbUptd, string tabUptd,
            string label, string colProd, string colUptd, IProgress<string>? progress = null)
        {
            // Test Valore Data e Record Data presenti in Uptd
            // Confrontandolo con la tabella 01_tabella_data del Db di produzione.

            string _connProd = await Conn.MysqlConn(dbProd);
            string _connUptd = await Conn.MysqlConn(dbUptd);

            if (string.IsNullOrEmpty(_connProd) || string.IsNullOrEmpty(_connUptd))
            {
                await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al database.", "OK");
                return false;
            }
            
            bool tuttoOk = true;

            string qProd = $"SELECT `{colProd}` FROM `{tabProd}` ORDER BY `{colProd}` ASC;";
            string qUptd = $"SELECT DISTINCT `{colUptd}` FROM `{tabUptd}` ORDER BY `{colUptd}` ASC;";

            progress?.Report($"Struct: {label}");

            var dataProd = new DataTable();
            await SqlAsync.SqlQryDataTable(_connProd, qProd, dataProd, 60);
            var dataUptd = new DataTable();
            await SqlAsync.SqlQryDataTable(_connUptd, qUptd, dataUptd, 60);

            // Rimuovo righe nulle da produzione.
            // Se non lo faccio mi da errore di confronto.

            dataProd = dataProd.AsEnumerable()
                .Where(r => !string.IsNullOrWhiteSpace(r[colProd]?.ToString()))
                .CopyToDataTable();

            progress?.Report($"Test: {label}");

            int countProd = dataProd.Rows.Count;
            int countUptd = dataUptd.Rows.Count;

            if (countUptd != countProd)
            {
                await Shell.Current.DisplayAlert("Test righe data", "Numero righe diverso tra produzione e uptd.", "OK");
                return false;
            }

            // Viene testato il valore delle date se corrispondenti.
            // Confronto i valori delle date tra produzione e uptd.

            var valoriProd = dataProd.AsEnumerable().Select(r => r[colProd]?.ToString()).ToHashSet();
            var valoriUptd = dataUptd.AsEnumerable().Select(r => r[colUptd]?.ToString()).ToHashSet();

            if (!valoriProd.SetEquals(valoriUptd))
            {
                bool correzione = await Shell.Current.DisplayAlert(
                    "Date non corrispondenti",
                    "I valori delle date tra tabella di produzione e aggiornamento non corrispondono. \nVuoi correggerli manualmente?",
                    "Sì", "No");

                if (!correzione)
                {
                    return false;                     
                }
                else
                {
                    //var vm = ServiceHelper.GetService<ModDataViewModel>();
                    await Shell.Current.GoToAsync(nameof(ModData));
                    return false; // fermiamo il test corrente, verrà rieseguito dopo la modifica
                }
            }
            return tuttoOk;
        }
        public static async Task<bool> TestNumCol(string dbProd, string tabProd, string dbUptd, string tabUptd,
            string label, IProgress<string>? progress = null)
        {
            string _connProd = await Conn.MysqlConn(dbProd);
            string _connUptd = await Conn.MysqlConn(dbUptd);
            
            if (string.IsNullOrEmpty(_connProd) || string.IsNullOrEmpty(_connUptd))
            {
                await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al database.", "OK");
                return false;
            }            

            bool tuttoOk = true;

            string showProd = $"SHOW COLUMNS FROM `{tabProd}`;";
            string showUptd = $"SHOW COLUMNS FROM `{tabUptd}`;";

            progress?.Report($"Test: {label}");

            var dataProd = new DataTable();
            await SqlAsync.SqlQryDataTable(_connProd, showProd, dataProd, 60);
            var dataUptd = new DataTable();
            await SqlAsync.SqlQryDataTable(_connUptd, showUptd, dataUptd, 60);

            if (dataUptd.Rows.Count != dataProd.Rows.Count)
            {
                await Shell.Current.DisplayAlert("Test colonne",
                    $"Numero colonne diverso tra produzione e uptd tabella: ({tabProd}).", "OK");
                return false;
            }
            return tuttoOk;
        }
    }
}
