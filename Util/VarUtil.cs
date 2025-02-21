namespace PvPmo.Util
{
    public class VarUtil
    {
        //Vengono lette le intestazioni delle colonne del foglio Excel attraverso (CreaTabDb) per
        //scrivere la query che crea le intestazioni della tabella nel db
        public static string CreaTabDb(string nomeTabDb, DataTable tabData)
        {
            int x = 0;
            string Qry = "CREATE TABLE " + nomeTabDb + " (";
            int i = tabData.Columns.Count - 1;

            foreach (DataColumn col in tabData.Columns)
            {
                string Name = col.ColumnName;
                string Type = col.DataType.ToString();
                Type = Type.Remove(0, 7);
                switch (Type)
                {
                    case "String":
                        Type = ("nvarchar(50)");
                        break;
                        //case "Double":
                        //    Type = ("INT");
                        //    break;
                }
                switch (Name)
                {
                    case "Disapproved Timesheets":
                        Type = ("INT(10) ZEROFILL NOT NULL");
                        break;
                    case "Overdue Timesheets":
                        Type = ("INT(10) ZEROFILL NOT NULL");
                        break;
                    case "Timesheets Submitted":
                        Type = ("INT(10) ZEROFILL NOT NULL");
                        break;
                    case "Timesheets To Approve":
                        Type = ("INT(10) ZEROFILL NOT NULL");
                        break;
                }

                if (x < i)
                {
                    Qry = Qry + "`" + Name + "` " + Type + ", ";
                }
                else
                {
                    Qry = Qry + "`" + Name + "` " + Type;
                }
                x++;
            }
            Qry += ");";
            return Qry;
        }
        // Leggo le intestazioni colonna della tabella temp_ e quelle della tabella di riferimento se il numero non
        // corrisponde esco con errore altrimenti passo le liste.
        public static string ComparaNomeColonna(string db, string nomeTabella)
        {
            DataTable TabTemp = new DataTable();
            DataTable TabRif = new DataTable();

            //ElencoQrySql.NomiColSql(db, "temp_" + nomeTabella, TabTemp);
            //ElencoQrySql.NomiColSql(db, nomeTabella, TabRif);
            //// Test numero colonne.
            //if (TabTemp.Rows.Count < TabRif.Rows.Count)
            //{
            //    Utilita.MessErr("La tabella importata ha un numero di colonne minore di quella del Db.",
            //    "Errore importazione Tabelle");
            //    return "";
            //}
            //else if (TabTemp.Rows.Count > TabRif.Rows.Count)
            //{
            //    Utilita.MessErr("La tabella del Db ha un numero di colonne minore di quella importata.",
            //    "Errore importazione Tabelle");
            //    return "";
            //}

            List<string> ListaTemp = new List<string>();
            DataRow[] temp = TabTemp.Select();
            for (int j = 0; j < temp.Length; j++)
            {
                ListaTemp.Add(temp[j]["Field"].ToString());
            }

            List<string> ListaRif = new List<string>();
            DataRow[] rif = TabTemp.Select();

            for (int j = 0; j < rif.Length; j++)
            {
                ListaRif.Add(rif[j]["Field"].ToString());
            }
            string Qqry = AggiornaTabellaDb(nomeTabella, ListaTemp, ListaRif);
            return Qqry;
        }
        // Creo la Qry per aggiornare la tabella presente nel Db con la tabella Excel temporanea importata nel Db
        // controllo se l'intestazione colonna corrispondono e più precisamente se la temporanea contiene
        // il nome di quella di riferimento.
        public static string AggiornaTabellaDb(string nomeTab, List<string> temp, List<string> rif)
        {
            string Qry = "INSERT INTO `" + nomeTab + "` (";
            string QryTabRif = "";
            string QryTabTemp = "";
            string TempLetto;
            string RifLetto;

            for (int j = 0; j < temp.Count; j++)
            {
                TempLetto = temp[j].ToString();
                RifLetto = rif[j].ToString();
                Boolean Contiene = TempLetto.Contains(RifLetto);
                // Test se input contiene Db
                if (Contiene == true)
                {
                    QryTabRif = QryTabRif + "`" + RifLetto + "`, ";
                    QryTabTemp = QryTabTemp + "`temp_" + nomeTab + "`.`" + TempLetto + "`, ";
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
}
