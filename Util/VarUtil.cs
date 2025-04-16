namespace PvPmo.Util;

public class VarUtil
{    
    // Leggo le intestazioni colonna della tabella temp_ e quelle della tabella di
    // riferimento se il numero non corrisponde esco con errore altrimenti passo le liste.
    public static string ComparaNomeColonna(string nomeTabella, DataTable _tabellaExl, DataTable _tabellaSql)
    {
        DataTable _tabExl = new DataTable();
        DataTable TabRif = new DataTable();

        //ElencoQrySql.NomiColSql(db, "temp_" + nomeTabella, TabTemp);
        //ElencoQrySql.NomiColSql(db, nomeTabella, TabRif);
        // Test numero colonne.
        

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
        //string Qqry = AggTabSql(nomeTabella, ListaExl, ListaSql);
        return "";
    }
    // Creo la Qry per aggiornare la tabella presente nel Db con la tabella Excel temporanea
    // importata nel Db controllo se l'intestazione colonna corrispondono e più
    // precisamente se la temporanea contiene il nome di quella di riferimento.
    public static string AggTabSql(string nomeTab, DataTable tabUptd, DataTable tabProd)
    {
        string Qry = "INSERT INTO pmo." + nomeTab + " (`";
        string QryTabRif = "";
        string QryTabTemp = "";
        string uptdLetto;
        string prodLetto;
        bool Bol = false;

        int x = tabProd.Rows.Count;

        DataRow[] tempProd = tabProd.Select("item");
        DataRow[] tempUptd = tabUptd.Select();
        for (int j = 0; j <= x; j++)
        {
            //prodLetto =
            //prodLetto = jtempProd["item"].ToString();
            //uptdLetto = tempUptd[j].ToString();
            //bool Contiene = uptdLetto.Contains(prodLetto);
            //// Test se input contiene Db
            //if (Contiene == true)
            //{
            //    QryTabRif = QryTabRif + "`" + prodLetto + "`, ";
            //    QryTabTemp = QryTabTemp + "`temp_" + nomeTab + "`.`" + uptdLetto + "`, ";
            //}
            //else
            //{
            //    Shell.Current.DisplayAlert
            //        ("Creazione Qry Update mensile Db produzione !",
            //        $"La righa in produzione non è contenuta in quella di Uptd.",
            //        "Ok");
            //    Bol = true;
            //}
        }
        QryTabRif = QryTabRif.Substring(0, QryTabRif.Length - 2);
        QryTabTemp = QryTabTemp.Substring(0, QryTabTemp.Length - 2);
        Qry = Qry + QryTabRif + ") pvpmo_origine. " + QryTabTemp + " FROM pvpmo_origine." + nomeTab + ";";
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
