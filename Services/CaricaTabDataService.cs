namespace PvPmo.Services
{
    public interface ICaricaTabDataService
    {
        Task<IList<CaricaTabData>> GetTabDataAsync(CancellationToken cancellationToken = default);
    }

    public class CaricaTabDataService : ICaricaTabDataService
    {
        private readonly ICaricaTabDataRepository _repository;

        public CaricaTabDataService(ICaricaTabDataRepository repository)
        {
            _repository = repository;
        }

        public async Task<IList<CaricaTabData>> GetTabDataAsync(CancellationToken cancellationToken = default)
        {
            return await _repository.GetAllAsync(cancellationToken);
        }
    }

}
