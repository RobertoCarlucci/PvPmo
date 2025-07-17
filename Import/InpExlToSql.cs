namespace PvPmo.Import
{
    public partial class InpExlToSql
    {
        // Importazione file Excel per aggiornamento mensile
        public static async Task<bool> InpExl(string type, IProgress<string>? progress = null)
        {
            SagaService _saga = new();

            string? exlPath = await SelCart.PickFolder();
            if (string.IsNullOrWhiteSpace(exlPath)) return false;

            bool inprtOk = true;
            string dbUptd = "pvpmo_origine";
            string dbProd = "pmo";
            string _connArch = string.Empty;            

            _connArch = (!string.IsNullOrEmpty(dbUptd)) ? _connArch = await Conn.MysqlConn(dbUptd) : _connArch;
            if (string.IsNullOrEmpty(_connArch))
            {
                await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al database.", "OK");
                return false;
            }

            var descrizioni = await ProgressHelper.CaricaDescrizioniAsync();
            var dati = await EmbeddedJsonLoader.LoadJsonAsync<OrigineConfig>("OrigineConfig.json");

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

                inprtOk = await ImpFileExltoTabSql(_saga, n.WorkSheet!, n.DbDest!, n.TabellaSql!, filePath, label, progress);
                if (inprtOk) 
                { 
                    current++; 
                }
                else
                {
                    await MostraErrore($"Errore su: {n.TabellaSql} non è possibile proseguire.");
                    await _saga.RollbackAsync(dbUptd);                    
                    return inprtOk;
                }
            }
            inprtOk = await NormTab.NormTabImp("EXL", dbUptd);
            if (!inprtOk)
            {
                await MostraErrore("Errore Normalizzazione Tabelle !\nNormalizzazione fallita .");
                await _saga.RollbackAsync(dbUptd);                
                return inprtOk;
            }
            inprtOk = await TestDateImpExl.TestUptd(progress);
            if (!inprtOk)
            {
                await MostraErrore("Errore test Tabelle update !\nControllare file Excell .");
                await _saga.RollbackAsync(dbUptd);
                return inprtOk;
            }
            inprtOk = await TestDateTab.VerificaDateProduzioneUpdateAsync(progress);
            if (!inprtOk)
            {
                await MostraErrore("Test Date tabelle produzione e update fallito.");
                await _saga.RollbackAsync(dbUptd);
                return inprtOk;
            }
            inprtOk = await UptdProd.EsgUptdTabProd("pmo", dbUptd, progress);
            if (!inprtOk)
            {
                await _saga.RollbackAsync(dbUptd);

                await MostraErrore("Aggiornamento delle tabelle della produzione fallito. " +
                    "\n Rieseguire la procedura dopo un controllo delle tabelle da importare.");
                return inprtOk;
            }
            inprtOk = await UpdtKeyOuts.UptdKeyOutsTransaction(dbProd, progress);
            if (!inprtOk)
            {
                await MostraErrore( "Update sulle tabelle Key & Outs falliti. \nNessuna modifica è stata effettuata sulla produzione.");
                return inprtOk;
            }
            inprtOk = await PulisciDb.ClearPriKey(progress);
            if (!inprtOk)
            {
                await MostraErrore("Update primary fallito. \nRicaricare il Db");
                return inprtOk;
            }
            await Shell.Current.DisplayAlert("Aggiornamento DB !", "Aggiornamento mensile completato!", "OK");
            return inprtOk;
        }
        public static async Task MostraErrore(string messaggio)
        {
            await Shell.Current.DisplayAlert("Errore Update Db !", messaggio, "OK");
        }

        public static async Task<bool> ImpFileExltoTabSql( SagaService _saga,
            string workSheet, string nomeDbSql, string nomeTbSql, string exlPath, string label, IProgress<string>? progress = null)
        {
            // Crea tabella SQL da file Excel (solo struttura)
            
            string _connSql = await Conn.MysqlConn(nomeDbSql);
            string _connExl = await Conn.ExlFileConn(exlPath);
            if (string.IsNullOrEmpty(_connSql) || string.IsNullOrEmpty(_connExl))
            {
                await Shell.Current.DisplayAlert("Errore Connessione", "Impossibile creare le connessioni.", "OK");
                return false;
            }            

            _saga.AggiungiStep(new ProteggiTabelleSagaStep(nomeTbSql));
            bool ok = await _saga.EseguiUltimoStepAsync(_connSql);
            if (!ok) return false;

            var _tabella = new DataTable();

            string qrySchema = $@"SELECT * FROM [{workSheet}$] WHERE 1=0;";
            progress?.Report($"Struct: {label}");
            ok = await ExlAsync.ExcQry(_connExl, qrySchema, _tabella);
            if (!ok || _tabella.Columns.Count == 0) return false;

            string ddl = $@"CREATE OR REPLACE TABLE " + NormTab.NormInp(nomeTbSql, _tabella);
            progress?.Report($"Struct: {label}");
            ok = await SqlAsync.SqlNoQryString(_connSql, ddl, 60);
            if (!ok) return false;
            
            progress?.Report($"Mapping: {label}");
            var Mappings = await DbUtlil.MyMapping("EXL", nomeDbSql, nomeTbSql, null, null, workSheet,  exlPath);
            if (Mappings == null) return false;

            // Copia i dati dal file Excel alla tabella SQL
            // Crea una tabella temporanea per i dati da importare in pmo.origine

            _tabella = new DataTable();

            string QryExl = $@"SELECT * FROM [{workSheet}$]";
            progress?.Report($"Load: {label}");
            ok = await ExlAsync.ExcQry(_connExl, QryExl, _tabella);
            if (!ok || _tabella.Rows.Count == 0) return false;

            progress?.Report($"Write: {label}");
            ok = await SqlAsync.SqlBulkCopy(_connSql, nomeTbSql, _tabella, Mappings, 60);
            return ok;            
        }
    }
}