namespace PvPmo.GestDb
{
    public partial class PulisciDb
    {
        public static async Task<bool> ClearPriKey(IProgress<string>? progress)
        {
            var descrizioni = await ProgressHelper.CaricaDescrizioniAsync();

            //    // Carico il File dall'archivio con la sequenza da svolgere
            //    // e provvedo all'esecuzione.

            var databaseService = ServiceHelper.GetService<DatabaseService>();
            var final = await databaseService.GetFinalizzaConfigsAsync();

            bool tuttoOK = true;
            string _connProd = string.Empty;

            foreach (var u in final.Where(u => u.Azione == "CLN"))
            {
                var db = string.IsNullOrWhiteSpace(u.DbTabConfronto) ? "default" : u.DbTabConfronto;
                var key = $"{u.TabConfronto}|{db}";
                var label = descrizioni.TryGetValue(key, out var desc) ? desc : $"{u.TabConfronto} ({db})";

                _connProd = (!string.IsNullOrEmpty(u.DbTabConfronto)) ? _connProd = await Conn.MysqlConn(u.DbTabConfronto) : _connProd;
                if (string.IsNullOrEmpty(_connProd))
                {
                    await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al database.", "OK");
                    return false;
                }

                progress?.Report($"Clean PKey: {label}");
                string dropid = $@"ALTER TABLE `{u.TabConfronto}` DROP COLUMN IF EXISTS id;";
                tuttoOK = await SqlAsync.SqlNoQryString(_connProd, dropid, 120);

                progress?.Report($"Clean PKey: {label}");
                string addIncrement = $@"ALTER TABLE `{u.TabConfronto}` AUTO_INCREMENT = 1;";
                tuttoOK = await SqlAsync.SqlNoQryString(_connProd, addIncrement, 180);

                progress?.Report($"Create PKey: {label}");
                string addId = $@"ALTER TABLE `{u.TabConfronto}` ADD id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY FIRST;";
                tuttoOK = await SqlAsync.SqlNoQryString(_connProd, addId, 0);

            }
            return tuttoOK;
        }
    }    
}