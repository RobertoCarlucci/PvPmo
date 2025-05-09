namespace PvPmo.Util
{
    public partial class NormTab
    {
        // Viene caricata la tabella "normalizza" dal Db "pvpmo_origine"
        // utilizzando dalla cartella Service il servizio "CaricaTabNorm" che mi
        // fornisce la mappatura delle colonne da modificare e se necessario anche la
        // stringa da inserire nel comando Sql CONCAT
        public static async Task<bool> NormTabImp(string tipoImportazione)
        {
            // Carico la tabella con le azioni da svolgere dal Service
            var repo = new CaricaTabRepository<CaricaTabNorm>("pvpmo_origine", "origine");

            var dati = await repo.GetAllAsync();
            
            bool Bol = false;
            // "tipoImportazione" identifica il tipo di file da lavorare ACS o EXL
    //        foreach (var n in _caricatabnorm.CaricaTabNorm)
    //        {
    //            string? _azione = n.Azione;
    //            string? _coldamod = n.ColDaMod;
    //            string? _modifica = n.Modifica;
    //            string? _tipocol = n.TipoCol;
    //            string? _tabellamod = n.TabellaMod;
    //            string? _dbdest = n.DbDest;
    //            string? _inptype = n.InpType;
    //            if (_inptype == tipoImportazione)
    //            {
    //                string StrConnSql = Conn.MysqlConn(_dbdest);
    //                switch (_azione)
    //                {
    //                    case "DEL":
    //                        Bol = await SqlQry.DelColSql(StrConnSql, _tabellamod, _coldamod);
    //                        break;
    //                    case "REN":
    //                        Bol = await SqlQry.RinColSql(StrConnSql, _tabellamod, _coldamod, _modifica, _tipocol);
    //                        break;
    //                    case "ADD":
    //                        Bol = await SqlQry.AddColSql(StrConnSql, _tabellamod, _coldamod, _tipocol);
    //                        break;
    //                    case "GEN":
    //                        switch (_coldamod)
    //                        {
    //                            case "Dateid":
    //                                Bol = await SqlQry.CreaDateId(StrConnSql, _tabellamod, _coldamod, _modifica);
    //                                break;
    //                            case "IdMonthYear":
    //                                Bol = await SqlQry.CreaIdMonthYear(StrConnSql, _tabellamod, _coldamod, _modifica);
    //                                break;
    //                            case "Keyid":
    //                                Bol = await SqlQry.CreaKeyId(StrConnSql, _tabellamod, _coldamod, _modifica);
    //                                break;
    //                        }
    //                        break;
    //                    }
    //                }
    //            }
            return Bol;
       }        
    }
}
