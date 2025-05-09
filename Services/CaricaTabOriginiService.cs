namespace PvPmo.Services
{
    public interface ICaricaTabOriginiService
    {
        Task<IList<CaricaTabOrigini>> GetOriginiAsync(CancellationToken cancellationToken = default);
    }
    public class CaricaTabOriginiService : ICaricaTabOriginiService
    {
        private readonly ICaricaTabOriginiRepository _repository;

        public CaricaTabOriginiService(ICaricaTabOriginiRepository repository)
        {
            _repository = repository;
        }

        public async Task<IList<CaricaTabOrigini>> GetOriginiAsync(CancellationToken cancellationToken = default)
        {
            return await _repository.GetAllAsync(cancellationToken);
        }
    }
}
