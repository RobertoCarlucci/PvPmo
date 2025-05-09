using Microsoft.Extensions.Configuration;

namespace PvPmo.Services
{
    public interface ICaricaTabNormService
    {
        Task<IList<CaricaTabNorm>> GetNormAsync(CancellationToken cancellationToken = default);
    }
    public class CaricaTabNormService : ICaricaTabNormService
    {
        private readonly CaricaTabRepository<CaricaTabNorm> _repository;

        public CaricaTabNormService(IConfiguration config)
        {
            _repository = new CaricaTabRepository<CaricaTabNorm>("pvpmo_origine", "normalizza");
        }

        public async Task<IList<CaricaTabNorm>> GetNormAsync(CancellationToken cancellationToken = default)
        {
            return await _repository.GetAllAsync(cancellationToken);
        }
    }


}
