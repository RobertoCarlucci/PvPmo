using Microsoft.Extensions.Configuration;

namespace PvPmo.Repositories
{
    public interface ICaricaTabFinalizzaRepository
    {
        Task<List<CaricaTabFinalizza>> GetAllAsync(CancellationToken cancellationToken = default);
    }
    public class CaricaTabFinalizzaRepository : ICaricaTabFinalizzaRepository
    {
        private readonly string _connectionString;

        public CaricaTabFinalizzaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("pvpmo_origine");
        }

        public async Task<List<CaricaTabFinalizza>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<CaricaTabFinalizza>();

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            const string query = "SELECT * FROM finalizza";
            await using var cmd = new MySqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new CaricaTabFinalizza
                {
                    Azione = reader["Azione"]?.ToString(),
                    TabConfronto = reader["TabConfronto"]?.ToString(),
                    ColConfronto = reader["ColConfronto"]?.ToString(),
                    TabTestare = reader["TabTestare"]?.ToString(),
                    ColDaTestare = reader["ColDaTestare"]?.ToString(),
                    DbTabConfronto = reader["DbTabConfronto"]?.ToString(),
                    DbTabTest = reader["DbTabTest"]?.ToString()
                });
            }
            return result;
        }
    }
}
