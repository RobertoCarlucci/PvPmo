namespace PvPmo.Services
{
    public class SagaService
    {
        private readonly SagaOrchestrator _orchestrator = new();

        public void AggiungiStep(ISagaStep step)
        {
            _orchestrator.AddStep(step);
        }

        public async Task<bool> EseguiAsync(string conn)
        {
            return await _orchestrator.ExecuteAsync(conn);
        }

        public async Task RollbackAsync(string conn)
        {
            await _orchestrator.RollbackAsync(conn);
        }

        //public bool HaStepCompletati => _orchestrator.HasExecuted;
    }

}
