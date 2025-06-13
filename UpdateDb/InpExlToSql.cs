namespace PvPmo.UpdateDb
{
    public partial class InpExlToSql()
    {
        // Importazione file Excel per aggiornamento mensile

        public static async Task<bool> InpExl(string type, IProgress<string>? progress = null)
        {
            string? exlPath = await SelCart.PickFolder();
            if (string.IsNullOrWhiteSpace(exlPath)) return false;

            bool inprtOk = true;

            var descrizioni = await DescrizioniProgress.GetProgressDescriptionsAsync(Conn.MysqlConn("pvpmo_origine"));
            
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
            inprtOk = await TestDateImpExl.FinalizzaUptd(progress);
            if (!inprtOk)
            {
                await Shell.Current.DisplayAlert("Errore Test Tabelle !", "Controlli sulle tabelle importate falliti. " +
                    "\nNessuna modifica è stata effettuata sulla produzione.", "OK");
                return inprtOk;
            }
            inprtOk = await UpdtKeyOuts.UptdKeyOutsTransaction("pmo", progress);
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

            string strConnSql = Conn.MysqlConn(nomeDbSql);

            if (nomeTbSql is "all_project_mapped_power_bi_column_set" or "timesheet_information_by_month") 
            {
                
                DataTable tab = new DataTable();

                string Connpmo = Conn.MysqlConn("pmo");
                string testTab = $@"SHOW TABLES LIKE '{nomeTbSql}';";                     
                
                await SqlAsync.SqlQryDataTable(Connpmo, testTab, tab, 60);
                if (tab.Rows.Count <= 0)
                // La tabella non esiste, quindi la creo.
                // Utilizzo la tabella di origine per creare la nuova tabella in pmo.
                {
                    string createSql = $@"CREATE TABLE `pmo`.`{nomeTbSql}` AS  SELECT * FROM 
                        `pvpmo_origine`.`{nomeTbSql}`;";
                    progress?.Report($"Write: {label}");
                    bool tuttoOk = await SqlAsync.SqlNoQry(strConnSql, createSql, 60, null);
                    if (!tuttoOk) return false;
                    string alterSql = $@"ALTER TABLE `pmo`.`{nomeTbSql}` MODIFY COLUMN id 
                                            INT UNSIGNED NOT NULL PRIMARY KEY AUTO_INCREMENT;";
                    progress?.Report($"Write: {label}");
                    tuttoOk = await SqlAsync.SqlNoQry(strConnSql, alterSql, 60, null);
                    if (!tuttoOk) return false;
                }
            }            
            DataTable schema = new();
            
            string strConnExl = Conn.ExlFileConn(exlPath);
            
            string qrySchema = $@"SELECT * FROM [{workSheet}$] WHERE 1=0;";
            progress?.Report($"Struct: {label}");
            bool schemaOk = await ExlAsync.ExcQry(strConnExl, qrySchema, schema);
            if (!schemaOk || schema.Columns.Count == 0) return false;

            string ddl = $@"CREATE OR REPLACE TABLE " + NormTab.NormInp(nomeTbSql, schema);
            progress?.Report($"Struct: {label}");
            bool ddlOk = await SqlAsync.SqlNoQry(strConnSql, ddl, 60);
            if (!ddlOk) return false;

            
            progress?.Report($"Mapping: {label}");
            Mappings = await DbUtlil.MyMapping("EXL", nomeDbSql, nomeTbSql, null, null, workSheet,  exlPath);
            if (Mappings == null) return false;

            // Copia i dati dal file Excel alla tabella SQL
            // Crea una tabella temporanea per i dati da importare in pmo.origine

            DataTable _tabella = new DataTable();

            string QryExl = $@"SELECT * FROM [{workSheet}$]";
            progress?.Report($"Load: {label}");
            bool letturaOk = await ExlAsync.ExcQry(strConnExl, QryExl, _tabella);
            if (!letturaOk || _tabella.Rows.Count == 0) return false;

            progress?.Report($"Write: {label}");
            bool bulkOk = await SqlAsync.SqlBulkCopy(strConnSql, nomeTbSql, _tabella, Mappings, 60);
            return bulkOk;            
        }
    }
}