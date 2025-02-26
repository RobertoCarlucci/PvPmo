namespace PvPmo.Util
{
    public class VarUtil
    {
        //Vengono normalizzati i nomi delle Tabelle Sql creando le stesse.
        //Si procede anche alla normalizzazione dei nomi colonna.
        public static string NormInp(string nomeTabDb, DataTable tabData)
        {
            // Chiamata alla funzione di normalizzazione nome tabella.
            nomeTabDb = NormNomeTab(nomeTabDb);
                       
            string Qry = nomeTabDb + " (";
            int x = 0;
            int i = tabData.Columns.Count - 1;

            foreach (DataColumn col in tabData.Columns)
            {
                string Name = col.ColumnName;
                string Type = col.DataType.ToString();
                Type = Type.Remove(0, 7);
                // Aggiungo colonna id se non presente.
                if( x == 0 && Name != "ID")
                {
                    string idType = ("INT UNSIGNED NOT NULL PRIMARY KEY AUTO_INCREMENT");
                    string idName = "id";
                    Qry = Qry + "`" + idName + "` " + idType + ", ";
                }
                // Select tipo dati colonne.
                switch (Type)
                {
                    case "String":
                        Type = ("nvarchar(50)");
                        break;
                    case "Double":
                        Type = ("DECIMAL(2) UNSIGNED ZEROFILL");
                        break;
                    case "Int16":
                        Type = ("DECIMAL(2) UNSIGNED ZEROFILL");
                        break;
                }
                // Select nomi colonne e creazione chiave primaria dove necessaria.
                switch (Name)
                {
                    case "ID":
                        Type = ("INT UNSIGNED NOT NULL PRIMARY KEY AUTO_INCREMENT");
                        Name = "id";
                        break;
                    case "Action Items, Completed (#)":
                        Name = "action_items_completed_val";
                        break;
                    case "Action Items, Completed (%)":
                        Name = "action_items_completed_perc";
                        break;
                    case "Date":
                        Name = "dateid";
                        break;
                    case "Key":
                        Name = "keyid";
                        break;
                    case "ID&Month&Year":
                        Name = "id_month_year";
                        break;
                    case "Work ID #":
                        Name = "work_id";
                        break;
                }
                // Eliminazione dei caratteri speciali possibili in access.
                string RemVirgola = Name.Replace(",", "");
                string RemTrattino = RemVirgola.Replace("-", "_");
                string RemSpazi = RemTrattino.Replace(" ", "_");
                string RemTondeIn = RemSpazi.Replace("(", "_");
                string RemTondeFn = RemTondeIn.Replace(")", "_");
                // Accodamento nella query dei nomi campi.
                // Viene usato il carattere ` (Alt + 96) per indicare tipo stringa nel
                // nome colonna.
                if (x < i)
                {
                    Qry = Qry + "`" + RemTondeFn + "` " + Type + ", ";
                }
                else
                {
                    Qry = Qry + "`" + RemTondeFn + "` " + Type;
                }
                x++;
            }
            Qry += ");";
            return Qry;
        }

        public static string NormNomeTab(string nomeTab)
        {
            switch (nomeTab)
            {
                case "All Project Mapped - Power BI Column Set":
                    nomeTab = "all_project_mapped_power_bi_column_set";
                    break;
                case "PBX_ AllProjectMappedPowerBIColumnSet":
                    nomeTab = "pbx_all_project_mapped_power_bi_column_set";
                    break;
                case "Resource Type":
                    nomeTab = "resource_type";
                    break;
                case "ScenarioRestoAnno":
                    nomeTab = "scenario_resto_anno";
                    break;
                case "Standard Activities":
                    nomeTab = "standard_activities";
                    break;
                case "Timesheet Information By Month":
                    nomeTab = "timesheet_information_by_month";
                    break;
                case "Working Hours by Day":
                    nomeTab = "working_hours_by_day";
                    break;
                case "GlobalTimesheetExtract":
                    nomeTab = "global_timesheet_extract";
                    break;
                case "Key":
                    nomeTab = "key_global";
                    break;
                case "PBX_TimesheetInformationByMonth":
                    nomeTab = "pbx_timesheet_information_by_month";
                    break;
                case "01_tabelladata":
                    nomeTab = "01_tabella_data";
                    break;
                case "pv_total_outsoremese":
                    nomeTab = "pv_total_outs_ore_mese";
                    break;
            }
            return nomeTab;
        }
        // Leggo le intestazioni colonna della tabella temp_ e quelle della tabella di
        // riferimento se il numero non corrisponde esco con errore altrimenti passo le liste.
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
