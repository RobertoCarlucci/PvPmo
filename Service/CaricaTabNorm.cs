namespace PvPmo.Service;

public class CaricaTabNorm
{
    public string? Azione { get; set; }
    public string? ColDaMod { get; set; }
    public string? Modifica {  get; set; }
    public string? TipoCol { get; set; }
    public string? TabellaMod { get; set; }
    public string? DbDest { get; set; }
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
                Azione = v["azione"].ToString(),
                ColDaMod = v["coldamod"].ToString(),
                Modifica = v["modifica"].ToString(),
                TipoCol = v["tipocol"].ToString(),
                TabellaMod = v["tabellamod"].ToString(),
                DbDest = v["dbdest"].ToString(),
                InpType = v["inptype"].ToString()
            });
        }
    }
    public IList<CaricaTabNorm> CaricaTabNorm => _caricatabnorm;
}
