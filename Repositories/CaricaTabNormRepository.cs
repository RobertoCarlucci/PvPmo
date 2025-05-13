using Microsoft.Extensions.Configuration;

namespace PvPmo.Repositories
{
    public interface ICaricaTabNormRepository
    {
        Task<List<CaricaTabNorm>> GetAllAsync(CancellationToken cancellationToken = default);
    }
    public class CaricaTabNormRepository : ICaricaTabNormRepository
    {
        private readonly string _connectionString;

        public CaricaTabNormRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("pvpmo_origine");
        }

        public async Task<List<CaricaTabNorm>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<CaricaTabNorm>();

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            const string query = "SELECT * FROM normalizza";
            await using var cmd = new MySqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new CaricaTabNorm
                {
                    Azione = reader["azione"]?.ToString(),
                    ColDaMod = reader["coldamod"]?.ToString(),
                    Modifica = reader["modifica"]?.ToString(),
                    TipoCol = reader["tipocol"]?.ToString(),
                    TabellaMod = reader["tabellamod"]?.ToString(),
                    DbDest = reader["dbdest"]?.ToString(),
                    InpType = reader["inptype"]?.ToString()
                });
            }
            return result;
        }
    }
}
