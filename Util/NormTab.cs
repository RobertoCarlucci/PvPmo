namespace PvPmo.Util
{
    public static class NormTab
    {
        // Viene caricata la tabella "normalizza" dal Db "pvpmo_origine"
        //        // utilizzando dalla cartella Service il servizio "CaricaTabNorm" che mi
        //        // fornisce la mappatura delle colonne da modificare e se necessario anche la
        //        // stringa da inserire nel comando Sql CONCAT
        public static async Task<bool> NormTabImp(string tipoImportazione)
        {
            // Carico la tabella con le azioni da svolgere dal Service
            var repo = new CaricaTabRepository<CaricaTabNorm>("pvpmo_origine", "normalizza");
            var dati = await repo.GetAllAsync();

            bool esitoGlobale = true;

            foreach (var n in dati)
            {
                // "tipoImportazione" identifica il tipo di file da lavorare ACS o EXL
                if (n.InpType != tipoImportazione)
                    continue;

                string connStr = Conn.MysqlConn(n.DbDest);

                bool esitoSingolo = n.Azione switch
                {
                    "DEL" => await SqlQry.DelColSql(connStr, n.TabellaMod, n.ColDaMod),
                    "REN" => await SqlQry.RinColSql(connStr, n.TabellaMod, n.ColDaMod, n.Modifica, n.TipoCol),
                    "ADD" => await SqlQry.AddColSql(connStr, n.TabellaMod, n.ColDaMod, n.TipoCol),
                    "GEN" => await EseguiGenerazione(connStr, n),
                    _ => true
                };
                esitoGlobale &= esitoSingolo;
            }
            return esitoGlobale;
        }
        private static async Task<bool> EseguiGenerazione(string conn, CaricaTabNorm n) =>
            n.ColDaMod switch
            {
                "Dateid" => await SqlQry.CreaDateId(conn, n.TabellaMod, n.ColDaMod, n.Modifica),
                "IdMonthYear" => await SqlQry.CreaIdMonthYear(conn, n.TabellaMod, n.ColDaMod, n.Modifica),
                "Keyid" => await SqlQry.CreaKeyId(conn, n.TabellaMod, n.ColDaMod, n.Modifica),
                _ => true
            };
    }    
}
