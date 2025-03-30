namespace PvPmo.Service;

public class CaricaTabNorm
{
    public string? Colonna { get; set; }
    public string? Azione { get; set; }        
    public string? Tabella { get; set; }
    public string? Dbdest { get; set; }
    public string? InpType { get; set; }

}
public interface ICaricaTabNorm
{
    IList<CaricaTabNorm> CaricaTabNorm { get; }
}

public class CaricaTabNormService : ICaricaTabNorm
{
    private List<CaricaTabNorm> _caricatabnorm;

    public CaricaTabNormService()
    {
        DataTable _elencoInp = new DataTable();
        _caricatabnorm = new List<CaricaTabNorm>();

        string StrConn = Conn.MysqlConn("pvpmo_origine");
        string Qry = "SELECT * From normalizza;";
        SqlSync.SqlQryDataTable(StrConn, Qry, _elencoInp);
        DataRow[] temp = _elencoInp.Select();
        foreach (DataRow v in temp)
        {
            _caricatabnorm.Add(new CaricaTabNorm()
            {
                Colonna = v["colonna"].ToString(),
                Azione = v["azione"].ToString(),
                Tabella = v["tabella"].ToString(),
                Dbdest = v["dbdest"].ToString(),
                InpType = v["inptype"].ToString()
            });
        }
    }
    public IList<CaricaTabNorm> CaricaTabNorm => _caricatabnorm;
}
