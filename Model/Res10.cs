namespace PvPmo.Model
{
    public class Res10
    {
        public string? Resource_Name { get; set; }
        public DateTime Start_Date { get; set; }
        public DateTime Term_Date { get; set; }
        public string? User_Name { get; set; }
        public string? User_Role { get; set; }
        public string? Organizational_Resource { get; set; }
        public string? Calendar { get; set; }
        public int Hours_Per_Week { get; set; }
        public string? Activity_Type { get; set; }
        public string? Competence_Concatenated_List { get; set; }
        public string? Competence_Primary_Value { get; set; }
        public string? Competence_2 { get; set; }
        public string? Cost_Centers { get; set; }
        public string? Direct_Supervisor_Name { get; set; }
        public string? Direct_Supervisor_Name_2 { get; set; }
        public int Disapproved_Timesheets { get; set; }
        public string? Email_Notifications_Enabled { get; set; }
        public string? Job_Location_Country { get; set; }
        public string? Job_Location_Region { get; set; }
        public string? Legal_Entity { get; set; }
        public string? Network_Authentication_Name { get; set; }
        public string? Organization_OBS { get; set; }
        public int Overdue_Timesheets { get; set; }
        public string? Parameters { get; set; }
        public string? Providing_Org { get; set; }
        public string? RES_Organization { get; set; }
        public string? RES_Seniority { get; set; }
        public string? Resource_Comments { get; set; }
        public int Resource_Depth { get; set; }
        public int Resource_Quantity { get; set; }
        public string? Resource_Types { get; set; }
        public string? Short_Name { get; set; }
        public string? Skill_Concatenated_List { get; set; }
        public string? Skil_Primary_Value { get; set; }
        public string? Sub_Org_OBS { get; set; }
        public string? Team_OBS { get; set; }
        public string? Teams_Concatenated_List { get; set; }
        public string? Teams_Primary_Value { get; set; }
        public int Timesheets_Submitted { get; set; }
        public int Timesheets_To_Approve { get; set; }
        public string? User_EMail_Address { get; set; }
        public string? User_Pager { get; set; }
        public string? User_Phone_1 { get; set; }
        public string? User_Phone_2 { get; set; }
        public string? Work_Location { get; set; }
        public DateTime? DateId { get; set; }
    }
    public interface IRes10Service
    {
        IList<Res10> InpRes10 { get; }
    }
    public class Res10Service : IRes10Service
    {
        private List<Res10> _inpres10 = null;

        public Res10Service()
        {
        //    DataTable _elencoInp = new DataTable();
            _inpres10 = new List<Res10>();

        //    string StrConn = Db.Conn.MysqlConn("pvpmo_origine");
        //    string Qry = "SELECT * From origine_acs;";
        //    Db.SqlDb.SqlQrySyn(StrConn, Qry, _elencoInp);
        //    DataRow[] temp = _elencoInp.Select();
        //    foreach (DataRow v in temp)
        //    {
        //        _inpres10.Add(new Res10()
        //        {
        //            db_inp = v["db_inp"].ToString(),
        //            tabella = v["tabella"].ToString(),
        //            db_dest = v["db_dest"].ToString(),
        //        });
        //    }
        }
        public IList<Res10> InpRes10 => _inpres10;
    }
}
