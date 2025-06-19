namespace PvPmo.Services
{
    public interface ICaricaTabProgressService
    {
        Task<IList<CaricaTabProgress>> GetOriginiAsync(CancellationToken cancellationToken = default);
    }
    public class CaricaTabProgressService : ICaricaTabProgressService
    {
        private readonly ICaricaTabProgressRepository _repository;

        public CaricaTabProgressService(ICaricaTabProgressRepository repository)
        {
            _repository = repository;
        }

        public async Task<IList<CaricaTabProgress>> GetOriginiAsync(CancellationToken cancellationToken = default)
        {
            return await _repository.GetAllAsync(cancellationToken);
        }
    }
}
