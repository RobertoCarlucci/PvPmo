namespace PvPmo.Util;

public class VarUtil
{    
    // Leggo le intestazioni colonna della tabella temp_ e quelle della tabella di
    // riferimento se il numero non corrisponde esco con errore altrimenti passo le liste.
    public static string ComparaNomeColonna(string nomeTabella, DataTable _tabellaExl, DataTable _tabellaSql)
    {
        //DataTable _tabellaExl = new DataTable();
        DataTable TabRif = new DataTable();

        //ElencoQrySql.NomiColSql(db, "temp_" + nomeTabella, TabTemp);
        //ElencoQrySql.NomiColSql(db, nomeTabella, TabRif);
        // Test numero colonne.
        if (_tabellaExl.Rows.Count < _tabellaSql.Rows.Count)
        {
            //Utilita.MessErr("La tabella importata ha un numero di colonne minore di quella del Db.",
            //"Errore importazione Tabelle");
            //return "";
        }
        else if (_tabellaExl.Rows.Count > _tabellaSql.Rows.Count)
        {
            //Utilita.MessErr("La tabella del Db ha un numero di colonne minore di quella importata.",
            //"Errore importazione Tabelle");
            //return "";
        }

        List<string> ListaExl = new List<string>();
        DataRow[] exl = _tabellaExl.Select();
        for (int j = 0; j < exl.Length; j++)
        {
            ListaExl.Add(exl[j]["Field"].ToString());
        }

        List<string> ListaSql = new List<string>();
        DataRow[] sql = _tabellaSql.Select();

        for (int j = 0; j < sql.Length; j++)
        {
            ListaSql.Add(sql[j]["Field"].ToString());
        }
        string Qqry = AggTabSql(nomeTabella, ListaExl, ListaSql);
        return Qqry;
    }
    // Creo la Qry per aggiornare la tabella presente nel Db con la tabella Excel temporanea
    // importata nel Db controllo se l'intestazione colonna corrispondono e più
    // precisamente se la temporanea contiene il nome di quella di riferimento.
    public static string AggTabSql(string nomeTab, List<string> listaExl, List<string> listaSql)
    {
        string Qry = "INSERT INTO `" + nomeTab + "` (";
        string QryTabRif = "";
        string QryTabTemp = "";
        string exlLetto;
        string sqlLetto;

        for (int j = 0; j < listaExl.Count; j++)
        {
            exlLetto = listaExl[j].ToString();
            sqlLetto = listaSql[j].ToString();
            Boolean Contiene = exlLetto.Contains(sqlLetto);
            // Test se input contiene Db
            if (Contiene == true)
            {
                QryTabRif = QryTabRif + "`" + sqlLetto + "`, ";
                QryTabTemp = QryTabTemp + "`temp_" + nomeTab + "`.`" + exlLetto + "`, ";
            }
            //else
            //{
            //    Utilita.MessErr("La tabella del Db ha il nome colonna: " + RifLetto +
            //    " non presente in quella imprtata.", "Errore importazione Tabelle");
            //    return "";
            //}
        }
        QryTabRif = QryTabRif.Substring(0, QryTabRif.Length - 2);
        QryTabTemp = QryTabTemp.Substring(0, QryTabTemp.Length - 2);
        Qry = Qry + QryTabRif + ") SELECT " + QryTabTemp + " FROM temp_" + nomeTab + ";";
        return Qry;
    }
    public static List<T> ConvDTtoList<T>(DataTable dt) where T : new()
    {
        List<T> list = new List<T>();
        foreach (DataRow row in dt.Rows)
        {
            T obj = new T();
            foreach (DataColumn col in dt.Columns)
            {
                var prop = obj.GetType().GetProperty(col.ColumnName);                    
                if (prop != null && row[col] != DBNull.Value)                        
                    prop.SetValue(obj, row[col]);
            }
            list.Add(obj);
        }
        return list;
    }
}
