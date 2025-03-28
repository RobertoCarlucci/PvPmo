using PvPmo.Service;

namespace PvPmo.UpdateDb
{
    public partial class NormTabExl
    {
        public static async Task<bool> NormTabImp()
        {
            LeggiTabDbNormExlService _normimp = new LeggiTabDbNormExlService();
            bool Bol = false;
            foreach (var n in _normimp.LeggiTabDbNormExl)
            {
                string? _colonna = n.Colonna;
                string? _azione = n.Azione;
                string? _tabella = n.Tabella;
                string? _db = n.Dbdest;
                string? _inptype = n.InpType;
                if (_tabella is not null && _inptype == "EXL")
                {
                    switch (_tabella)
                    {
                        case "pv_total":
                            Bol = await PvTotalUptd(_db, _tabella, _colonna, _azione);
                            break;
                        case "global_timesheet_extract":
                            Bol = await GlobalTimesExtrUptd(_db, _tabella, _colonna, _azione);
                            break;
                        case "timesheet_information_by_month":
                            Bol = await TimesheetInformationbyMonthUptd(_db, _tabella, _colonna, _azione);
                            break;
                    }
                }
            }
            return Bol;
        }
        // Se il File è Timesheet Information by Month aggiungo la colonna necessaria
        // e popolo la stessa con i dati attraverso le opportune query.
        public static async Task<bool> TimesheetInformationbyMonthUptd(string nomeDbSql, string nomeTabSql, string nomeColSql, string azione)
        {
            string StrConnSql = Conn.MysqlConn(nomeDbSql);
            bool Bol = false;
            if (nomeTabSql == "timesheet_information_by_month" && azione == "ADD")
            {
                switch (nomeColSql)
                {
                    case "id_month_year":
                        Bol = await SqlQry.AddColSql(StrConnSql, nomeColSql, nomeTabSql, "nvarchar(50)");
                        string concat = "`Short Name`, LPAD(MONTH(`Date`),2,0), YEAR(`Date`)";
                        Bol = await SqlQry.CreaIdMonthYear(StrConnSql, nomeTabSql, nomeColSql, concat);
                        break;
                }
            }
            return true;
        }
        // Se il File è Global Timesheer Extract aggiungo le colonne necessarie e popolo 
        // le stesse con i dati attraverso le opportune query.
        public static async Task<bool> GlobalTimesExtrUptd(string nomeDbSql, string nomeTabSql, string nomeColSql, string azione)
        {
            string StrConnSql = Conn.MysqlConn(nomeDbSql);
            bool Bol = false;
            if (nomeTabSql == "global_timesheet_extract" && azione == "ADD")
            {
                switch (nomeColSql)
                {
                    case "dateid":
                        Bol = await SqlQry.AddColSql(StrConnSql, nomeColSql, nomeTabSql, "nvarchar(50)");
                        string concat = "'01', LPAD(`Timesheet_Month`,2,0), `Timesheet_Year`";
                        Bol = await SqlQry.CreaDateId(StrConnSql, nomeTabSql, nomeColSql, concat);
                        break;
                    case "id_month_year":
                        Bol = await SqlQry.AddColSql(StrConnSql, nomeColSql, nomeTabSql, "nvarchar(50)");
                        concat = "`GEC`, LPAD(`Timesheet_Month`,2,0), `Timesheet_Year`";
                        Bol = await SqlQry.CreaIdMonthYear(StrConnSql, nomeTabSql, nomeColSql, concat);
                        break;
                    case "keyid":
                        Bol = await SqlQry.AddColSql(StrConnSql, nomeColSql, nomeTabSql, "nvarchar(100)");
                        concat = "`Organization`,`Providing_Org`,`Team`,`Competence`,`Location_Region`";
                        Bol = await SqlQry.CreaKeyId(StrConnSql, nomeTabSql, nomeColSql, concat);
                        break;
                }
            }
            return true;
        }
        // Se il File è PV_Total aggiungo le colonne necessarie e popolo 
        // le stesse con i dati attraverso le opportune query, imoltre rimuovo
        // le collonne che non vengono utilizzate.
        public static async Task<bool> PvTotalUptd(string nomeDbSql, string nomeTabSql, string nomeColSql, string azione)
        {
            string StrConnSql = Conn.MysqlConn(nomeDbSql);
            bool Bol = false;
            if (nomeTabSql == "pv_total" && azione == "ADD")
            {
                switch (nomeColSql)
                {
                    case "dateid":
                        Bol = await SqlQry.RinColSql(StrConnSql, nomeTabSql, "Date", nomeColSql, "DATE");
                        break;
                    case "id_month_year":
                        Bol = await SqlQry.AddColSql(StrConnSql, nomeColSql, nomeTabSql, "nvarchar(50)");
                        string concat = "`Short Name`, LPAD(MONTH(`Date`),2,0), YEAR(`Date`)";
                        Bol = await SqlQry.CreaIdMonthYear(StrConnSql, nomeTabSql, nomeColSql, concat);
                        break;
                    case "keyid":
                        Bol = await SqlQry.AddColSql(StrConnSql, nomeColSql, nomeTabSql, "nvarchar(100)");
                        concat = "`Organization (OBS)`,`Providing Org#`,`Team (OBS)`,`Competence (Primary Value)`,`Job Location Region`";
                        Bol = await SqlQry.CreaKeyId(StrConnSql, nomeTabSql, nomeColSql, concat);
                        break;
                }
            }
            else if (nomeTabSql == "pv_total" && azione == "DEL")
            {
                Bol = await SqlQry.DelColSql(StrConnSql, nomeTabSql, nomeColSql);
            }
            ;
            return Bol;
        }
    }
}
