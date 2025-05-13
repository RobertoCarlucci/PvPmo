using PvPmo.ImportDb;

namespace PvPmo.UpdateDb
{
    public partial class InpExlToSql
    {
        // Importazione file Excel per aggiornamento mensile
        public static async Task InpExl()
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
                }
                await NormTab.NormTabImp("EXL");
                await TestDateImpExl.FinalizzaUptd();
            }            
        }

        // Crea tabella SQL da file Excel (solo struttura)
        public static async Task<bool> NomeColFileExltoTabSql(string nomeFoglioExl, string nomeDbSql, string nomeTbSql, string exlPath)
        {
            DataTable tabellaExl = new();
            string strConnExl = Conn.ExlFileConn(exlPath);
            string strConnSql = Conn.MysqlConn(nomeDbSql);
            string qryExl = $"SELECT * FROM [{nomeFoglioExl}$] WHERE 1=0;";

            bool ok = await ExlAsync.ExcQry(strConnExl, qryExl, tabellaExl);
            if (!ok || tabellaExl.Columns.Count == 0)
                return false;

            string ddl = "CREATE OR REPLACE TABLE " + NormTab.NormInp(nomeTbSql, tabellaExl);
            return await SqlAsync.SqlNoQry(strConnSql, ddl, 30);
        }

        // Copia i dati dal file Excel alla tabella SQL
        public static async Task<bool> DatiFileExltoTabSql(string nomeFoglio, string nomeDbSql, string nomeTbSql, string exlPath)
        {
            DataTable tabella = new();
            string strConnExl = Conn.ExlFileConn(exlPath);
            string qryExl = $"SELECT * FROM [{nomeFoglio}$];"; // Carica dati reali
            bool ok = await ExlAsync.ExcQry(strConnExl, qryExl, tabella);
            if (!ok || tabella.Rows.Count == 0)
                return false;

            string strConnSql = Conn.MysqlConn(nomeDbSql);
            return await SqlAsync.SqlBulkCopy(strConnSql, nomeTbSql, tabella, 120);
        }
    }
}
