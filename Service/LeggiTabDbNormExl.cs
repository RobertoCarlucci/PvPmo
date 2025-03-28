namespace PvPmo.Service;

public class LeggiTabDbNormExl
{
    public string? Colonna { get; set; }
    public string? Azione { get; set; }        
    public string? Tabella { get; set; }
    public string? Dbdest { get; set; }
    public string? InpType { get; set; }

}
public interface ILeggiTabDbNormExl
{
    IList<LeggiTabDbNormExl> LeggiTabDbNormExl { get; }
}

public class LeggiTabDbNormExlService : ILeggiTabDbNormExl
{
    private List<LeggiTabDbNormExl> _leggitabdbnormexl;

    public LeggiTabDbNormExlService()
    {
        DataTable _elencoInp = new DataTable();
        _leggitabdbnormexl = new List<LeggiTabDbNormExl>();

        string StrConn = Conn.MysqlConn("pvpmo_origine");
        string Qry = "SELECT * From normalizza;";
        SqlSync.SqlQryDataTable(StrConn, Qry, _elencoInp);
        DataRow[] temp = _elencoInp.Select();
        foreach (DataRow v in temp)
        {
            _leggitabdbnormexl.Add(new LeggiTabDbNormExl()
            {
                Colonna = v["colonna"].ToString(),
                Azione = v["azione"].ToString(),
                Tabella = v["tabella"].ToString(),
                Dbdest = v["dbdest"].ToString(),
                InpType = v["inptype"].ToString()
            });
        }
    }
    public IList<LeggiTabDbNormExl> LeggiTabDbNormExl => _leggitabdbnormexl;
}
