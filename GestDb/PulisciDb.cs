namespace PvPmo.GestDb
{
    public partial class PulisciDb
    {
        public static async Task<bool> ClearPriKey(IProgress<string>? progress)
        {
            var descrizioni = await DescrizioniProgress.GetProgressDescriptionsAsync(await Conn.MysqlConn("pvpmo_origine"));

            //    // Carico il File dall'archivio con la sequenza da svolgere
            //    // e provvedo all'esecuzione.

            var repo = new CaricaTabRepository<CaricaTabFinalizza>("pvpmo_origine", "finalizza");
            var dati = await repo.GetAllAsync();

            bool tuttoOK = true;
            string _connProd = string.Empty;

            foreach (var u in dati.Where(u => u.Azione == "CLN"))
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
                tuttoOK = await SqlAsync.SqlNoQry(_connProd, dropid, 60);

                progress?.Report($"Clean PKey: {label}");
                string addIncrement = $@"ALTER TABLE `{u.TabConfronto}` AUTO_INCREMENT = 1;";
                tuttoOK = await SqlAsync.SqlNoQry(_connProd, addIncrement, 180);

                progress?.Report($"Create PKey: {label}");
                string addId = $@"ALTER TABLE `{u.TabConfronto}` ADD id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY FIRST;";
                tuttoOK = await SqlAsync.SqlNoQry(_connProd, addId, 1800);

            }
            return tuttoOK;
        }        
    }
}
