using DocumentFormat.OpenXml.InkML;
using System.Collections.Generic;
using System.Text;

namespace PvPmo.Import;

public partial class InpAcsToSql()
{
    //Importazione dei due Db Access con i loro dati.
    public static async Task NewDb(string type, IProgress<string>? progress)
    {
        string? acsPath = await SelCart.PickFolder();
        if (string.IsNullOrWhiteSpace(acsPath)) return;
        
        var descrizioni = await DescrizioniProgress.GetProgressDescriptionsAsync(await Conn.MysqlConn("pvpmo_origine"));

        var repo = new CaricaTabRepository<CaricaTabOrigini>("pvpmo_origine", "origine");
        var dati = await repo.GetAllAsync();

        // Aggiunto per evitare che un errore blocchi tutte le tabelle:

        var validi = dati.Where(n =>
                n.InpType == type &&
                !string.IsNullOrWhiteSpace(n.DbInp) &&
                !string.IsNullOrWhiteSpace(n.Tabella) &&
                !string.IsNullOrWhiteSpace(n.DbDest) &&
                !string.IsNullOrWhiteSpace(n.TabellaSql)).ToList();

        int total = validi.Count * 2;
        int current = 0;

        foreach (var n in validi)
        {
            var db = string.IsNullOrWhiteSpace(n.DbDest) ? "default" : n.DbDest;
            var key = $"{n.TabellaSql}|{db}";
            var label = descrizioni.TryGetValue(key, out var desc) ? desc : $"{n.TabellaSql} ({db})";

            try
            {
                if(!string.IsNullOrEmpty(n.DbInp) && !string.IsNullOrEmpty(n.Tabella) 
                    && !string.IsNullOrEmpty(n.DbDest) && !string.IsNullOrEmpty(n.TabellaSql)) 
                {
                    bool ok1 = await TabAcstoTabSql(n.DbInp, n.Tabella, n.DbDest, n.TabellaSql, acsPath, progress, label);
                    if (ok1) { current++; }

                    bool ok2 = await InpAcsDatitoSql(n.DbInp, n.Tabella, n.DbDest, n.TabellaSql, acsPath, progress, label);
                    if (ok2) { current++; }

                    if (!ok1 || !ok2)
                    {
                        await Shell.Current.DisplayAlert("Errore", $"Errore durante l'import di {n.Tabella}", "OK");
                    }
                }              
                
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Errore", $"Tabella {n.Tabella}: {ex.Message}", "OK");
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
    public static async Task<bool> TabAcstoTabSql(
        string nomeDbAcs, string nomeTbAcs, string nomeDbSql, string nomeTbSql, string acsPath, IProgress<string>? progress, string label)
    {
        string _connProd = string.Empty;
        string _connAcs = string.Empty;

        _connProd = (!string.IsNullOrEmpty(nomeDbSql)) ? _connProd = await Conn.MysqlConn(nomeDbSql) : _connProd;
        if (string.IsNullOrEmpty(_connProd))
        {
            await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al database.", "OK");
            return false;
        }
        
        _connAcs = (!string.IsNullOrEmpty(nomeDbAcs) || !string.IsNullOrEmpty(acsPath)) ? _connAcs = await Conn.AcsDbConn(nomeDbAcs, acsPath) : _connAcs;
        if (string.IsNullOrEmpty(_connAcs))
        {
            await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al database.", "OK");
            return false;
        }

        DataTable _tabella = new DataTable();
        string Qry = $"SELECT * FROM [{nomeTbAcs}] WHERE 1=0;";
        progress?.Report($"Read: {label}");
        await AcsAsync.AcsQryTab(_connAcs, Qry, _tabella);
        Qry = NormTab.NormInp(nomeTbSql, _tabella);
        Qry = "CREATE OR REPLACE TABLE " + Qry;        
        progress?.Report($"Create: {label}");
        bool Bol = await SqlAsync.SqlNoQryString(_connProd, Qry, 30);
        return Bol;
    }

    // Importo i dati all'interno del Db andando a popolare con i valori le tabelle
    // colonne precedentemente create.

    public static async Task<bool> InpAcsDatitoSql(
        string nomeDbAcs, string nomeTbAcs, string nomeDbSql, string nomeTbSql, string acsPath, IProgress<string>? progress, string label)
    {
        List<MySqlBulkCopyColumnMapping> Mappings = new List<MySqlBulkCopyColumnMapping>();
        string _connProd = string.Empty;
        string _connAcs = string.Empty;

        progress?.Report($"Stuct: {label}");
        Mappings = await DbUtlil.MyMapping("ACS", nomeDbSql, nomeTbSql, nomeDbAcs, nomeTbAcs, null, acsPath);

        if (Mappings == null)
            return false;

        _connProd = (!string.IsNullOrEmpty(nomeDbSql)) ? _connProd = await Conn.MysqlConn(nomeDbSql) : _connProd;
        if (string.IsNullOrEmpty(_connProd))
        {
            await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al database.", "OK");
            return false;
        }

        _connAcs = (!string.IsNullOrEmpty(nomeDbAcs) || !string.IsNullOrEmpty(acsPath)) ? _connAcs = await Conn.AcsDbConn(nomeDbAcs, acsPath) : _connAcs;
        if (string.IsNullOrEmpty(_connAcs))
        {
            await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al database.", "OK");
            return false;
        }

        DataTable tabella = new();
        string qry = $"SELECT * FROM [{nomeTbAcs}]";

        await AcsAsync.AcsQryTab(_connAcs, qry, tabella);
        progress?.Report($"Write: {label}");
        return await SqlAsync.SqlBulkCopy(_connProd, nomeTbSql, tabella, Mappings, 180);
    }    
}