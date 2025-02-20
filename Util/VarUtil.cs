namespace PvPmo.Util
{
    public class VarUtil
    {
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
