namespace PvPmo.Models
{
    public interface ISagaStep
    {
        string StepName { get; }
        Task<bool> ExecuteAsync(string conn, MySqlTransaction? trans);
        Task CompensateAsync(string conn);
    }
    public partial class SagaOrchestrator
    {
        private readonly List<ISagaStep> _steps = new();
        private readonly Stack<ISagaStep> _completati = new();
        public void AddStep(ISagaStep step) => _steps.Add(step);
        public async Task<bool> ExecuteAsync(string conn)
        {
            _completati.Clear();

            foreach (var step in _steps)
            {
                bool ok = await step.ExecuteAsync(conn, null);

                if (!ok)
                {
                    await RollbackAsync(conn);
                    return false;
                }
                _completati.Push(step);
            }
            return true;
        }

        public async Task RollbackAsync(string conn)
        {            
            while (_completati.Count > 0)
            {
                var step = _completati.Pop();                
                await step.CompensateAsync(conn);
            }
            await Shell.Current.DisplayAlert("Procedura di ripristino !", $"Db ripristinato ai dati iniziali .", "OK");
        }
    }
    public partial class ProteggiTabelleSagaStep : ISagaStep
    {
        private readonly string _connString;
        private readonly string _tabella;
        public string StepName => $"Backup tabella [{_tabella}]";
        public ProteggiTabelleSagaStep(string connString, string nomeTabella)
        {
            _connString = connString;
            _tabella = nomeTabella;
        }
        public async Task<bool> ExecuteAsync(string conn, MySqlTransaction? trans)
        {
            var renameQuery = $@"
            DROP TABLE IF EXISTS {_tabella}_shw;
            RENAME TABLE {_tabella} TO {_tabella}_shw;";
                        
            return await SqlAsync.SqlNoQry(conn, renameQuery, 60);
        }
        public async Task CompensateAsync(string conn)
        {
            var rollbackQuery = $@"
            DROP TABLE IF EXISTS {_tabella};
            RENAME TABLE {_tabella}_shw TO {_tabella};";
                        
            await SqlAsync.SqlNoQry(conn, rollbackQuery, 60);            
        }
    }
}
