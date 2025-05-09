using Microsoft.Extensions.Configuration;

namespace PvPmo.Repositories
{
    public interface ICaricaTabDataRepository
    {
        Task<List<CaricaTabData>> GetAllAsync(CancellationToken cancellationToken = default);
    }

    public class CaricaTabDataRepository : ICaricaTabDataRepository
    {
        private readonly string _connectionString;

        public CaricaTabDataRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("pmo");
        }

        public async Task<List<CaricaTabData>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<CaricaTabData>();

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            const string query = "SELECT * FROM `01_tabella_data`";
            await using var cmd = new MySqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new CaricaTabData
                {
                    PV_TotalData = reader["PV_TotalData"] as DateTime?,
                    GlobalTimesheetExtractData = reader["GlobalTimesheetExtractData"] as DateTime?,
                    PV_MeseSuAnno = reader["PV_MeseSuAnno"] as DateTime?
                });
            }
            return result;
        }
    }
}
