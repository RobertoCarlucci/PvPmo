namespace PvPmo.Services
{
    public interface ICaricaTabFinalizzaService
    {
        Task<IList<CaricaTabFinalizza>> GetFinalizzaAsync(CancellationToken cancellationToken = default);
    }
    public class CaricaTabFinalizzaService : ICaricaTabFinalizzaService
    {
        private readonly ICaricaTabFinalizzaRepository _repository;

        public CaricaTabFinalizzaService(ICaricaTabFinalizzaRepository repository)
        {
            _repository = repository;
        }

        public async Task<IList<CaricaTabFinalizza>> GetFinalizzaAsync(CancellationToken cancellationToken = default)
        {
            return await _repository.GetAllAsync(cancellationToken);
        }
    }
}
