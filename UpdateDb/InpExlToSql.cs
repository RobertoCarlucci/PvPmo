using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using PvPmo.ImportDb;

namespace PvPmo.UpdateDb
{
    public partial class InpExlToSql
    {
        // Importazione file Excel per aggiornamento mensile
        public static async Task InpExl(IProgress<double> progress = null)
        {
            string? exlPath = await SelCart.PickFolder();
            if (string.IsNullOrWhiteSpace(exlPath))
            {
                return;
            }
            else
            {
                var repo = new CaricaTabRepository<CaricaTabOrigini>("pvpmo_origine", "origine");
                var dati = await repo.GetAllAsync();

                int total = dati.Count;
                int current = 0;

                foreach (var n in dati)
                {
                    if (n.InpType != "EXL") continue;

                    if (string.IsNullOrWhiteSpace(n.Tabella) || string.IsNullOrWhiteSpace(n.DbDest) ||
                        string.IsNullOrWhiteSpace(n.TabellaSql) || string.IsNullOrWhiteSpace(n.WorkSheet))
                        continue;

                    string filePath = Path.Combine(exlPath, n.Tabella);

                    bool ok1 = await NomeColFileExltoTabSql(n.WorkSheet, n.DbDest, n.TabellaSql, filePath);
                    bool ok2 = await DatiFileExltoTabSql(n.WorkSheet, n.DbDest, n.TabellaSql, filePath);

                    if (!ok1 || !ok2)
                    {
                        await Shell.Current.DisplayAlert("Errore", $"Errore su tabella: {n.TabellaSql}", "OK");
                    }
                    current++;
                    progress?.Report(current / (double)total); // ✅ AGGIORNATO QUI
                }
                bool norm = await NormTab.NormTabImp("EXL");
                if (!norm)
                {
                    await Shell.Current.DisplayAlert("Errore", "Normalizzazione fallita", "OK");
                    return;
                }
                else 
                {
                    await TestDateImpExl.FinalizzaUptd();
                }                
            }
            await Shell.Current.DisplayAlert("Aggiornamento DB", "Aggiornamento mensile completato!", "OK");
        }

        // Crea tabella SQL da file Excel (solo struttura)
        public static async Task<bool> NomeColFileExltoTabSql(string workSheet, string nomeDbSql, string nomeTbSql, string exlPath)
        {
            DataTable schema = new();

            string strConnExl = Conn.ExlFileConn(exlPath);
            string strConnSql = Conn.MysqlConn(nomeDbSql);

            string qrySchema = $"SELECT * FROM [{workSheet}$] WHERE 1=0;";
            bool schemaOk = await ExlAsync.ExcQry(strConnExl, qrySchema, schema);
            if (!schemaOk || schema.Columns.Count == 0) return false;

            string ddl = "CREATE OR REPLACE TABLE " + NormTab.NormInp(nomeTbSql, schema);
            bool ddlOk = await SqlAsync.SqlNoQry(strConnSql, ddl, 30);
            if (!ddlOk) return false;

            bool mappingOk = await NormTab.CreaMappingExlSql(strConnExl, strConnSql, workSheet, nomeDbSql, nomeTbSql);
            if (!mappingOk) return false;

            return mappingOk;
        }

        // Copia i dati dal file Excel alla tabella SQL
        public static async Task<bool> DatiFileExltoTabSql(string workSheet, string nomeDbSql, string nomeTbSql, string exlPath)
        {
            DataTable dati = new();
            string strConnExl = Conn.ExlFileConn(exlPath);
            string qryExl = $"SELECT * FROM [{workSheet}$];"; // Carica dati reali
            bool ok = await ExlAsync.ExcQry(strConnExl, qryExl, dati);
            if (!ok || dati.Rows.Count == 0) return false;

            string strConnSql = Conn.MysqlConn(nomeDbSql);
            bool insertOk = await SqlAsync.SqlBulkCopy(strConnSql, nomeTbSql, dati, 120);
            if (!insertOk) return false;
            return insertOk;
        }
    }
}
