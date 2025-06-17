using DocumentFormat.OpenXml.InkML;
using MySqlConnector;

namespace PvPmo.GestDb
{
    public partial class SqlMapp
    {
        private static Dictionary<string, List<MySqlBulkCopyColumnMapping>> mapsDict;

        public static void AddListMapping(string nome, int sourceOrdinal, string destinationColumn)
        {
            // Fix for CS0029: Correctly add the mapping to the dictionary instead of assigning a List to a string variable
            if (mapsDict == null) { mapsDict = new Dictionary<string, List<MySqlBulkCopyColumnMapping>>(); }

            if (!mapsDict.ContainsKey(nome)) { mapsDict[nome] = new List<MySqlBulkCopyColumnMapping>(); }

            mapsDict[nome].Add(new MySqlBulkCopyColumnMapping
            {
                SourceOrdinal = sourceOrdinal,
                DestinationColumn = destinationColumn
            });
        }

        //private static List<MySqlBulkCopyColumnMapping> Mappings = new List<MySqlBulkCopyColumnMapping>();

        public static List<MySqlBulkCopyColumnMapping> ApplicaMapping(MySqlBulkCopy bulkCopy, string nomeMapping)
        {
            if (!mapsDict.ContainsKey(nomeMapping))
            {
                throw new ArgumentException($"Profilo di mapping '{nomeMapping}' non trovato.", nameof(nomeMapping));
            }

            var mappingDaApplicare = mapsDict[nomeMapping];

            // FONDAMENTALE: Pulisci sempre i mapping precedenti prima di aggiungerne di nuovi
            bulkCopy.ColumnMappings.Clear();

            // Aggiungi i mapping del profilo scelto
            foreach (var mapping in mappingDaApplicare)
            {
                bulkCopy.ColumnMappings.Add(mapping);
            }
            //Console.WriteLine($"Applicato il profilo di mapping: '{nomeMapping}'");
            return mappingDaApplicare;
        }

        public static async Task<Dictionary<string, List<MySqlBulkCopyColumnMapping>>> MyMappingList(string nMapping, string dbDest, string tabDest,
            string? dbOrgn = null, string? tbOrgn = null, string? nomeWorkSheet = null, string? filePath = null)
        {
            DataTable dTabOrgn = new();
            DataTable dTabDest = new();
            int dif = 0;

            string conndbDest = Conn.MysqlConn(dbDest);
            string connOrgn = Conn.MysqlConn(dbOrgn);

            string qryProd = $"SHOW COLUMNS FROM `{tabDest}`;";
            await SqlAsync.SqlQryDataTable(conndbDest, qryProd, dTabDest);

            string qryUptd = $@"SHOW COLUMNS FROM `{tbOrgn}`;";
            await SqlAsync.SqlQryDataTable(connOrgn, qryUptd, dTabOrgn);

            dif = dTabDest.Rows.Count - dTabOrgn.Rows.Count;

            if (dif == 0)
            {
                for (int i = 0; i < dTabOrgn.Rows.Count && i < dTabDest.Rows.Count; i++)
                {
                    if (dTabDest.Rows[i]["Field"].ToString() != "id")
                    {
                        string? destCol = dTabDest.Rows[i]["Field"]?.ToString();
                        AddListMapping(nMapping, i, destCol ?? $"Row{i}");
                    }
                }
                return mapsDict;
            }

            var confermaOk = await Shell.Current.DisplayAlert("Errore colonne",
                $"Le colonne nella tabella di origine: {dbOrgn} - ({dTabOrgn.Columns.Count}) e nella tabella di destinazione" +
                $": {dbDest} - ({dTabDest.Rows.Count}) non coincidono. Vuoi continuare con le altre tabelle?", "Si", "No");

            dif = dTabDest.Rows.Count - dTabOrgn.Columns.Count;

            if (dif == 0 || dif == 1)
            {
                int offset = dif == 1 ? 1 : 0;

                for (int i = 0; i < dTabOrgn.Columns.Count && (i + offset) < dTabDest.Rows.Count; i++)
                {
                    string? destCol = dTabDest.Rows[i + offset]["Field"]?.ToString();
                    AddListMapping(nMapping, i, destCol ?? $"Col{i + offset}");
                }
            }

            var conferma = await Shell.Current.DisplayAlert("Errore colonne",
                $"Le colonne nella tabella di origine: {dbOrgn} - ({dTabOrgn.Columns.Count}) e nella tabella di destinazione" +
                $": {dbDest} - ({dTabDest.Rows.Count}) non coincidono. Vuoi continuare con le altre tabelle?", "Si", "No");

            return mapsDict;
        }
    }
}
