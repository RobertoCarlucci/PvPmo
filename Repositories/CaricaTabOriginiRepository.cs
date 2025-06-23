using Microsoft.Extensions.Configuration;

namespace PvPmo.Repositories
{
    public interface ICaricaTabOriginiRepository
    {
        Task<List<CaricaTabOrigini>> GetAllAsync(CancellationToken cancellationToken = default);
    }
    public class CaricaTabOriginiRepository : ICaricaTabOriginiRepository
    {
        public interface ICaricaTabNormRepository
        {
            Task<List<CaricaTabOrigini>> GetAllAsync(CancellationToken cancellationToken = default);
        }
        private readonly string? _connectionString;

        public CaricaTabOriginiRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("pvpmo_origine");
        }

        public async Task<List<CaricaTabOrigini>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<CaricaTabOrigini>();

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            const string query = "SELECT * FROM origine";
            await using var cmd = new MySqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new CaricaTabOrigini
                {
                    DbInp = reader["DbInp"]?.ToString(),
                    Tabella = reader["Tabella"]?.ToString(),
                    DbDest = reader["DbDest"]?.ToString(),
                    TabellaSql = reader["TabellaSql"]?.ToString(),
                    WorkSheet = reader["WorkSheet"]?.ToString(),
                    InpType = reader["InpType"]?.ToString()
                });
            }

            return result;
        }
    }
}
