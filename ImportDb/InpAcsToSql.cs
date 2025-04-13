using PvPmo.Service;

namespace PvPmo.ImportDb;

public partial class InpAcsToSql
{
    //Importazione dei due Db Access con i loro dati.
    public static async Task NewDb()
    {
        // Apro una finestra di sistema x la selezione della cartella di importazione.
        string _acsPath = await SelCart.PickFolderStatic(default);
        CaricaTabOriginiService _inpdbacs = new CaricaTabOriginiService();

        foreach (var n in _inpdbacs.CaricaTabOrigini)
        {
            string? nomeDbAcs = n.DbInp;
            string? nomeTb = n.Tabella;
            string? nomeDbSql = n.DbDest;
            string? nomeTabSql = n.TabellaSql;
            string? nomeWorkSheet = n.WorkSheet;
            string? inpType = n.InpType;
            if (inpType == "ACS")
            {
                bool tab = await TabAcstoTabSql(nomeDbAcs, nomeTb, nomeDbSql, nomeTabSql, _acsPath);
                tab = await InpAcsDatitoSql(nomeDbAcs, nomeTb, nomeDbSql, nomeTabSql, _acsPath);
            }
        }
        bool Bol = await NormTab.NormTabImp("ACS");
    }
    // Creo le tabelle Sql leggendo i nomi delle tabelle Access e normalizzando le
    // intestazioni delle colonne in modo compatibile con sql.
    public static async Task<bool> TabAcstoTabSql(string nomeDbAcs, string nomeTbAcs, string nomeDbSql, string nomeTbSql, string acsPath)
    {
        string StrConnAcs = Conn.AcsDbConn(nomeDbAcs, acsPath);
        DataTable _tabella = new DataTable();
        string Qry = "SELECT * FROM [" + nomeTbAcs + "] WHERE 1=0";
        await AcsAsync.AcsQryTab(StrConnAcs, Qry, _tabella);
        Qry = NormInp(nomeTbAcs, nomeTbSql, _tabella);
        Qry = "CREATE OR REPLACE TABLE " + Qry;
        string StrConnSql = Conn.MysqlConn(nomeDbSql);
        bool Bol = await SqlAsync.SqlNoQry(StrConnSql, Qry, 30);
        return Bol;
    }
    // Importo i dati all'interno del Db andando a popolare con i valori le tabelle
    // colonne precedentemente create.
    public static async Task<bool> InpAcsDatitoSql(string nomeDbAcs, string nomeTbAcs, string nomeDbSql, string nomeTbSql, string acsPath)
    {
        bool Bol = await CreaMappingAcsSql(nomeDbAcs, nomeTbAcs, nomeDbSql, nomeTbSql, acsPath);
        if (Bol == true)
        {
            string StrConnAcs = Conn.AcsDbConn(nomeDbAcs, acsPath);
            DataTable _tabella = new DataTable();
            string Qry = "SELECT * FROM [" + nomeTbAcs + "]";
            await AcsAsync.AcsQryTab(StrConnAcs, Qry, _tabella);
            string StrConnSql = Conn.MysqlConn(nomeDbSql);
            await SqlAsync.SqlBulkCopy(StrConnSql, nomeTbSql, _tabella, 180);            
        }
        return Bol;
    }

    // Vengono lette le intestazioni delle colonne Access e Sql per creare il mapping
    // delle corrispondenze tra le due e di conseguenza caricato attraverso il metodo
    // AddMapping all'interno della cartella GestDb classe gestione Sql.
    public static async Task<bool> CreaMappingAcsSql(string nomeDbAcs, string nomeTbAcs, string nomeDbSql, string nomeTbSql, string acsPath)
    {
        DataTable _tabAcs = new DataTable();
        DataTable _tabSql = new DataTable();
        string QryAcs = "SELECT * FROM [" + nomeTbAcs + "] WHERE 1=0;";
        string QrySql = "SELECT * FROM `information_schema`.`COLUMNS` " +
            "WHERE TABLE_SCHEMA = '" + nomeDbSql + "' AND TABLE_NAME = " +
            "'" + nomeTbSql + "' ORDER BY ORDINAL_POSITION;";

        string StrConnAcs = Conn.AcsDbConn(nomeDbAcs, acsPath);
        string StrConnSql = Conn.MysqlConn(nomeDbSql);

        await AcsAsync.AcsQryTab(StrConnAcs, QryAcs, _tabAcs);
        await SqlAsync.SqlQryDataReader(StrConnSql, QrySql, _tabSql, 30);

        int _dif = _tabSql.Rows.Count - _tabAcs.Columns.Count;

        if (_dif == 0)
        {
            DataRow[] _rowSql = _tabAcs.Select();
            for (int x = 1; x < _tabSql.Rows.Count; x++)
            {
                int SourceOrdinal = x;
                string? DestinationColumn = _tabSql.Rows[x]["COLUMN_NAME"].ToString();
                SqlAsync.AddMapping(SourceOrdinal, DestinationColumn);
            }
            return true;
        }
        else if (_dif == 1)
        {
            DataRow[] _rowSql = _tabAcs.Select();
            for (int x = 1; x < _tabSql.Rows.Count; x++)
            {
                int SourceOrdinal = x - 1;
                string? DestinationColumn = _tabSql.Rows[x]["COLUMN_NAME"].ToString();
                SqlAsync.AddMapping(SourceOrdinal, DestinationColumn);
            }
            return true;
        }
        else if (_dif < 0 || _dif > 1)
        {
            var Conf = await Shell.Current.DisplayAlert
                ("Errore nel numero di Colonne delle Tabelle.", "La Tabella Access: "
                + _tabAcs + " Ha un numero di colonne diverso dalla tabella Sql: "
                + _tabSql + " Vuoi continuare ad importare le altre tabelle residue ? "
                , "Si", "No");
            if (Conf == true)
                return true;
            else

                return false;
        }
        return false;
    }
    // Vengono normalizzati i nomi delle Tabelle Sql creando le stesse.
    // Si procede anche alla normalizzazione dei nomi colonna.
    public static string NormInp(string nomeTabNorm, string nomeTbSql, DataTable tabData)
    {
        // Chiamata alla funzione di normalizzazione nome tabella.
        string nomeTabDb = nomeTabNorm;

        string Qry = nomeTbSql + " (";
        int x = 0;
        int i = tabData.Columns.Count - 1;

        foreach (DataColumn col in tabData.Columns)
        {
            string Name = col.ColumnName;
            string Type = col.DataType.ToString();
            Type = Type.Remove(0, 7);
            // Aggiungo colonna id se non presente.
            if (x == 0 && Name != "ID")
            {
                string idType = ("INT UNSIGNED NOT NULL PRIMARY KEY AUTO_INCREMENT");
                string idName = "id";
                Qry = Qry + "`" + idName + "` " + idType + ", ";
            }
            // Select tipo dati colonne.
            switch (Type)
            {
                case "String":
                    Type = ("nvarchar(50)");
                    break;
                case "Double":
                    Type = ("SMALLINT UNSIGNED");
                    break;
                case "Int16":
                    Type = ("SMALLINT UNSIGNED");
                    break;
                case "Single":
                    Type = ("SMALLINT UNSIGNED");
                    break;
                case "DateTime":
                    Type = ("DATE");
                    break;
            }
            // Select nomi colonne e creazione chiave primaria dove necessaria.
            switch (Name)
            {
                case "ID":
                    Type = ("INT UNSIGNED NOT NULL PRIMARY KEY AUTO_INCREMENT");
                    Name = "id";
                    break;
                case "Divisore":
                    Type = ("FLOAT");
                    break;
                case "Modifica":
                    Type = ("nvarchar(120)");
                    break;
                case "TipoCol":
                    Type = ("nvarchar(120)");
                    break;
                case "ORE":
                    Type = ("FLOAT");
                    Name = "Ore";
                    break;
                case "Hours Per Week":
                    Type = ("FLOAT");
                    break;
                case "Timesheet_Time":
                    Type = ("FLOAT");
                    break;
                case "Action Items, Completed (#)":
                    Name = "ActionItemsCompletedVal";
                    break;
                case "Action Items, Completed (%)":
                    Name = "ActionItemsCompletedPerc";
                    break;
                case "Date":
                    Name = "Dateid";
                    break;
                case "DateID":
                    Name = "Dateid";
                    break;
                case "Date ID":
                    Name = "Dateid";
                    break;
                case "DateKeY":
                    Name = "Datekey";
                    break;
                case "KeyFTE_Mese":
                    Name = "KeyFteMese";
                    break;
                case "Key":
                    Name = "Keyid";
                    break;
                case "KeyID":
                    Name = "Keyid";
                    break;
                case "ID&Month&Year":
                    Name = "IdMonthYear";
                    break;
                case "Work ID #":
                    Name = "WorkId";
                    break;
            }
            // Eliminazione dei caratteri speciali possibili in access.
            string RemVirgola = Name.Replace(",", "");
            string RemTrattinoAlto = RemVirgola.Replace("-", "");
            string RemTrattinoBasso = RemTrattinoAlto.Replace("_", "");
            string RemSpazi = RemTrattinoBasso.Replace(" ", "");
            string RemTondeIn = RemSpazi.Replace("(", "");
            string RemTondeFn = RemTondeIn.Replace(")", "");
            // Accodamento nella query dei nomi campi.
            // Viene usato il carattere ` (Alt + 96) per indicare tipo stringa nel
            // nome colonna.
            if (x < i)
            {
                Qry = Qry + "`" + RemTondeFn + "` " + Type + ", ";
            }
            else
            {
                Qry = Qry + "`" + RemTondeFn + "` " + Type;
            }
            x++;
        }
        Qry += ");";
        return Qry;
    }
}