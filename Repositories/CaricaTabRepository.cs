namespace PvPmo.Repositories
{
    public class CaricaTabRepository<T> where T : ICaricabileDaDataReader, new()
    {
        private readonly string _connectionString;
        private readonly string _tableName;

        public CaricaTabRepository(string dbName, string tableName)
        {
            _connectionString = Conn.MysqlConn(dbName);
            _tableName = tableName;
        }

        public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<T>();
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            var query = $"SELECT * FROM `{_tableName}`";
            await using var cmd = new MySqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var item = new T();
                item.FromReader(reader);
                result.Add(item);
            }
            return result;
        }
    }
}
