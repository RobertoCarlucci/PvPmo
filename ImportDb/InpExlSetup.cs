using DocumentFormat.OpenXml.Vml;
using PvPmo.UpdateDb;

namespace PvPmo.ImportDb
{
    public partial class InpExlSetup
    {
        public static async Task NewSetup()
        {
            // Apro una finestra di sistema x la selezione della cartella di importazione.
            string _exlPath = await SelCart.PickFolderStatic(default);             
            
            bool Bol = await ImportSetup(_exlPath, "NormalizzaInp", "Foglio 1", "pvpmo_origine", "normalizza");
            Bol = await ImportSetup(_exlPath, "Origine", "Foglio1", "pvpmo_origine", "origine");
        }
        public static async Task<bool> ImportSetup(string _exlPath, string nomeFoglioExl, string nomeWorkSheet, string nomeDbSql, string nomeTbSql)
        {
            DataTable _tabellaExl = new DataTable();

            string exlPath = _exlPath + "\\" + nomeFoglioExl;
            string StrConnExl = Conn.ExlFileConn(exlPath);
            string StrConnSql = Conn.MysqlConn(nomeDbSql);

            string QryExl = "SELECT * FROM [" + nomeWorkSheet + "$] WHERE 1=0;";
            bool Bol = ExlSync.ExcQry(StrConnExl, QryExl, _tabellaExl);
            string QrySql = InpAcsToSql.NormInp(nomeFoglioExl, nomeTbSql, _tabellaExl);
            QrySql = "CREATE OR REPLACE TABLE " + QrySql;
            Bol = await SqlAsync.SqlNoQry(StrConnSql, QrySql);
            Bol = await CreaMappingExlSql(StrConnExl, StrConnSql, nomeWorkSheet, nomeDbSql, nomeTbSql);
            QryExl = "SELECT * FROM [" + nomeWorkSheet + "$];";
            _tabellaExl = new DataTable();
            Bol = ExlSync.ExcQry(StrConnExl, QryExl, _tabellaExl);
            StrConnSql = Conn.MysqlConn(nomeDbSql);
            await SqlAsync.SqlBulkCopy(StrConnSql, nomeTbSql, _tabellaExl);
            return false;
        }
        public static async Task<bool> CreaMappingExlSql(string StrConnExl, string StrConnSql, string nomeWorkSheet, string nomeDbSql, string nomeTbSql)
        {
            DataTable _tabExl = new DataTable();
            DataTable _tabSql = new DataTable();
            string QryExl = "SELECT * FROM [" + nomeWorkSheet + "$] WHERE 1=0;";
            string QrySql = "SELECT * FROM `information_schema`.`COLUMNS` " +
                "WHERE TABLE_SCHEMA = '" + nomeDbSql + "' AND TABLE_NAME = " +
                "'" + nomeTbSql + "' ORDER BY ORDINAL_POSITION;";

            ExlSync.ExcQry(StrConnExl, QryExl, _tabExl);
            await SqlAsync.SqlQryDataReader(StrConnSql, QrySql, _tabSql);

            int _dif = _tabSql.Rows.Count - _tabExl.Columns.Count;

            if (_dif == 0)
            {
                DataRow[] _rowSql = _tabExl.Select();
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
                DataRow[] _rowSql = _tabExl.Select();
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
                    + _tabExl + " Ha un numero di colonne diverso dalla tabella Sql: "
                    + _tabSql + " Vuoi continuare ad importare le altre tabelle residue ? "
                    , "Si", "No");
                if (Conf == true)
                    return true;
                else

                    return false;
            }
            return false;
        }
    }
}
