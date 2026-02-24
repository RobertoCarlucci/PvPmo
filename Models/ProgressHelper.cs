namespace PvPmo.Models
{
    public static class ProgressHelper
    {
        public static async Task<Dictionary<string, string>> CaricaDescrizioniAsync()
        {
            var databaseService = ServiceHelper.GetService<DatabaseService>();
            var lista = await databaseService.GetAllAsync<ProgressConfig>();

            return lista
                .Where(p => !string.IsNullOrWhiteSpace(p.TabellaSql) &&
                            !string.IsNullOrWhiteSpace(p.DbName) &&
                            !string.IsNullOrWhiteSpace(p.Descrizione))
                .ToDictionary(
                    p => $"{p.TabellaSql}|{p.DbName}",
                    p => p.Descrizione!
                );
        }
    }
}
