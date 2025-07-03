namespace PvPmo.Services
{
    public class SagaService
    {
        private readonly SagaOrchestrator _orchestrator = new();

        public void AggiungiStep(ISagaStep step) => _orchestrator.AddStep(step);
        public void Reset() => _orchestrator.ClearSteps();

        public Task<bool> EseguiAsync(string conn) => _orchestrator.ExecuteAsync(conn);
        public Task<bool> EseguiUltimoStepAsync(string conn) => _orchestrator.ExecuteUltimoStepAsync(conn);
        public Task RollbackAsync(string conn) => _orchestrator.RollbackAsync(conn);
    }
}
