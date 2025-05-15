using System.Text;

namespace PvPmo.ImportDb;

public partial class InpAcsToSql
{
    //Importazione dei due Db Access con i loro dati.
    public static async Task NewDb()
    {
        string? acsPath = await SelCart.PickFolder();
        if (string.IsNullOrWhiteSpace(acsPath))
        {
            return;
        }
        else
        {
            var repo = new CaricaTabRepository<CaricaTabOrigini>("pvpmo_origine", "origine");
            var dati = await repo.GetAllAsync();

            foreach (var n in dati)
            {
                if (n.InpType != "ACS") continue;

                if (string.IsNullOrWhiteSpace(n.DbInp) || string.IsNullOrWhiteSpace(n.Tabella) ||
                    string.IsNullOrWhiteSpace(n.DbDest) || string.IsNullOrWhiteSpace(n.TabellaSql))
                    continue;
                // Aggiunto per evitare che un errore blocchi tutte le tabelle:
                try
                {
                    bool ok1 = await TabAcstoTabSql(n.DbInp, n.Tabella, n.DbDest, n.TabellaSql, acsPath);
                    bool ok2 = await InpAcsDatitoSql(n.DbInp, n.Tabella, n.DbDest, n.TabellaSql, acsPath);

                    if (!ok1 || !ok2)
                    {
                        await Shell.Current.DisplayAlert("Errore", $"Errore durante l'import di {n.Tabella}", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Errore", $"Tabella {n.Tabella}: {ex.Message}", "OK");
                }
            }
        }        
        bool norm = await NormTab.NormTabImp("ACS", "pmo");
        if (!norm) 
        {
            await Shell.Current.DisplayAlert("Errore", "Normalizzazione fallita", "OK");
        }
        else
        {
            await Shell.Current.DisplayAlert("Hai completato l'importazione dei dati nel Db !",
                    $"Importazione de dati da Access completata .", "Ok");
        }        
    }

    // Creo le tabelle Sql leggendo i nomi delle tabelle Access e normalizzando le
    // intestazioni delle colonne in modo compatibile con sql.
    public static async Task<bool> TabAcstoTabSql(string nomeDbAcs, string nomeTbAcs, string nomeDbSql, string nomeTbSql, string acsPath)
    {
        string StrConnAcs = Conn.AcsDbConn(nomeDbAcs, acsPath);
        DataTable _tabella = new DataTable();
        string Qry = $"SELECT * FROM [{nomeTbAcs}] WHERE 1=0;";        
        await AcsAsync.AcsQryTab(StrConnAcs, Qry, _tabella);
        Qry = NormTab.NormInp(nomeTbSql, _tabella);
        Qry = "CREATE OR REPLACE TABLE " + Qry;
        string StrConnSql = Conn.MysqlConn(nomeDbSql);
        bool Bol = await SqlAsync.SqlNoQry(StrConnSql, Qry, 30);
        return Bol;
    }

    // Importo i dati all'interno del Db andando a popolare con i valori le tabelle
    // colonne precedentemente create.

    public static async Task<bool> InpAcsDatitoSql(string nomeDbAcs, string nomeTbAcs, string nomeDbSql, string nomeTbSql, string acsPath)
    {
        bool mappingOk = await NormTab.CreaMappingAcsSql(nomeDbAcs, nomeTbAcs, nomeDbSql, nomeTbSql, acsPath);

        if (!mappingOk)
            return false;

        string connAcs = Conn.AcsDbConn(nomeDbAcs, acsPath);
        string connSql = Conn.MysqlConn(nomeDbSql);

        DataTable tabella = new();
        string qry = $"SELECT * FROM [{nomeTbAcs}]";

        await AcsAsync.AcsQryTab(connAcs, qry, tabella);
        return await SqlAsync.SqlBulkCopy(connSql, nomeTbSql, tabella, 180);
    }    
}