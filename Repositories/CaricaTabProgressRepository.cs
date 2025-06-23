using Microsoft.Extensions.Configuration;

namespace PvPmo.Repositories
{
    public interface ICaricaTabProgressRepository
    {
        Task<List<CaricaTabProgress>> GetAllAsync(CancellationToken cancellationToken = default);
    }
    public class CaricaTabProgressRepository : ICaricaTabProgressRepository
    {
        private readonly string? _connectionString;

        public CaricaTabProgressRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("pvpmo_origine");
        }

        public async Task<List<CaricaTabProgress>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<CaricaTabProgress>();

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            const string query = "SELECT * FROM progress_descrizione";
            await using var cmd = new MySqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new CaricaTabProgress
                {
                    TabellaSql = reader["TabellaSql"]?.ToString(),
                    DbName = reader["DbName"]?.ToString(),
                    Descrizione = reader["Descrizione"]?.ToString()                    
                });
            }
            return result;
        }
    }    
}
