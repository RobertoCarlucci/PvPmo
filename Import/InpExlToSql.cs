namespace PvPmo.Import
{
    public partial class InpExlToSql()
    {
        // Importazione file Excel per aggiornamento mensile

        public static async Task<bool> InpExl(string type, IProgress<string>? progress = null)
        {
            string? exlPath = await SelCart.PickFolder();
            if (string.IsNullOrWhiteSpace(exlPath)) return false;

            bool inprtOk = true;

            string _connArch = string.Empty;            

            _connArch = (!string.IsNullOrEmpty("pvpmo_origine")) ? _connArch = await Conn.MysqlConn("pvpmo_origine") : _connArch;
            if (string.IsNullOrEmpty(_connArch))
            {
                await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al database.", "OK");
                return false;
            }

            var descrizioni = await DescrizioniProgress.GetProgressDescriptionsAsync(_connArch);
            
            var repo = new CaricaTabRepository<CaricaTabOrigini>("pvpmo_origine", "origine");
            var dati = await repo.GetAllAsync();

            var validi = dati.Where(n =>
                n.InpType == type &&
                !string.IsNullOrWhiteSpace(n.Tabella) &&
                !string.IsNullOrWhiteSpace(n.DbDest) &&
                !string.IsNullOrWhiteSpace(n.TabellaSql) &&
                !string.IsNullOrWhiteSpace(n.WorkSheet)).ToList();

            int total = validi.Count * 2;
            int current = 0;

            foreach (var n in validi)
            {
                var db = string.IsNullOrWhiteSpace(n.DbDest) ? "default" : n.DbDest;
                var key = $"{n.TabellaSql}|{db}";
                var label = descrizioni.TryGetValue(key, out var desc) ? desc : $"{n.TabellaSql} ({db})";                        

                string filePath = Path.Combine(exlPath, n.Tabella!);                

                inprtOk = await impFileExltoTabSql(n.WorkSheet!, n.DbDest!, n.TabellaSql!, filePath, label, progress);
                if (inprtOk) 
                { 
                    current++; 
                }
                else
                {
                    await Shell.Current.DisplayAlert("Errore su tabella!", $"Errore su: {n.TabellaSql} " +
                        $"non è possibile proseguire.", "OK");
                    return inprtOk;
                }
            }
            inprtOk = await NormTab.NormTabImp("EXL", "pvpmo_origine");
            if (!inprtOk)
            {
                await Shell.Current.DisplayAlert("Errore Normalizzazione Tabelle !", "Normalizzazione fallita. " +
                    "\nNessuna modifica è stata effettuata sulla produzione.", "OK");
                return inprtOk;
            }
            inprtOk = await TestDateImpExl.TestUptd(progress);
            if (!inprtOk)
            {
                await Shell.Current.DisplayAlert("Errore Test Tabelle !", "Controlli sulle tabelle importate falliti. " +
                    "\nNessuna modifica è stata effettuata sulla produzione.", "OK");
                return inprtOk;
            }
            inprtOk = await UptdProd.EsgUptdTabProd("pmo", "pvpmo_origine", progress);
            if (!inprtOk)
            {
                await Shell.Current.DisplayAlert("Errore Aggiornamento Tabelle !", "Aggiornamento delle tabelle della produzione fallito. " +
                    "\nNessuna modifica è stata effettuata sulla produzione. " +
                    "\n Rieseguire la procedura dopo un controllo delle tabelle da importare.", "OK");
                return inprtOk;
            }
            inprtOk = await UpdtKeyOuts.UptdKeyOutsTransaction("pmo", progress);
            if (!inprtOk)
            {
                await Shell.Current.DisplayAlert("Errore Update !", "Update sulle tabelle Key & Outs falliti. " +
                    "\nNessuna modifica è stata effettuata sulla produzione.", "OK");
                return inprtOk;
            }
            inprtOk = await PulisciDb.ClearPriKey(progress);
            if (!inprtOk)
            {
                await Shell.Current.DisplayAlert("Errore Update !", "Update sulle tabelle Key & Outs falliti. " +
                    "\nNessuna modifica è stata effettuata sulla produzione.", "OK");
                return inprtOk;
            }
            await Shell.Current.DisplayAlert("Aggiornamento DB !", "Aggiornamento mensile completato!", "OK");
            return inprtOk;
        }              

        public static async Task<bool> impFileExltoTabSql(
            string workSheet, string nomeDbSql, string nomeTbSql, string exlPath, string label, IProgress<string>? progress = null)
        {
            // Crea tabella SQL da file Excel (solo struttura)

            List<MySqlBulkCopyColumnMapping> Mappings = new List<MySqlBulkCopyColumnMapping>();

            string _connUptd = string.Empty;
            string _connExl = string.Empty;

            _connUptd = (!string.IsNullOrEmpty(nomeDbSql)) ? _connUptd = await Conn.MysqlConn(nomeDbSql) : _connUptd;
            if (string.IsNullOrEmpty(_connUptd))
            {
                await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al database.", "OK");
                return false;
            }
            
            _connExl = (!string.IsNullOrEmpty(exlPath)) ? _connExl = await Conn.ExlFileConn(exlPath) : _connExl;
            if (string.IsNullOrEmpty(_connExl))
            {
                await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al File.", "OK");
                return false;
            }


            DataTable schema = new();

            string qrySchema = $@"SELECT * FROM [{workSheet}$] WHERE 1=0;";
            progress?.Report($"Struct: {label}");
            bool schemaOk = await ExlAsync.ExcQry(_connExl, qrySchema, schema);
            if (!schemaOk || schema.Columns.Count == 0) return false;

            string ddl = $@"CREATE OR REPLACE TABLE " + NormTab.NormInp(nomeTbSql, schema);
            progress?.Report($"Struct: {label}");
            bool ddlOk = await SqlAsync.SqlNoQry(_connUptd, ddl, 60);
            if (!ddlOk) return false;
            
            progress?.Report($"Mapping: {label}");
            Mappings = await DbUtlil.MyMapping("EXL", nomeDbSql, nomeTbSql, null, null, workSheet,  exlPath);
            if (Mappings == null) return false;

            // Copia i dati dal file Excel alla tabella SQL
            // Crea una tabella temporanea per i dati da importare in pmo.origine

            DataTable _tabella = new DataTable();

            string QryExl = $@"SELECT * FROM [{workSheet}$]";
            progress?.Report($"Load: {label}");
            bool letturaOk = await ExlAsync.ExcQry(_connExl, QryExl, _tabella);
            if (!letturaOk || _tabella.Rows.Count == 0) return false;

            progress?.Report($"Write: {label}");
            bool bulkOk = await SqlAsync.SqlBulkCopy(_connUptd, nomeTbSql, _tabella, Mappings, 60);
            return bulkOk;            
        }
    }
}