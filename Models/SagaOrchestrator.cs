namespace PvPmo.Models;

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
    public void ClearSteps()
    {
        _steps.Clear();
        _completati.Clear();
    }

    // Esegue tutti gli step in ordine
    public async Task<bool> ExecuteAsync(string conn)
    {
        _completati.Clear();

        foreach (var step in _steps)
        {
            Console.WriteLine($"▶️ Eseguo: {step.StepName}");
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

    // Esegue solo l’ultimo step aggiunto
    public async Task<bool> ExecuteUltimoStepAsync(string conn)
    {
        _completati.Clear();

        if (_steps.Count == 0)
            return true;

        var step = _steps.Last();
        Console.WriteLine($"▶️ Eseguo solo ultimo step: {step.StepName}");

        bool ok = await step.ExecuteAsync(conn, null);

        if (!ok)
        {
            await RollbackAsync(conn);
            return false;
        }

        _completati.Push(step);
        return true;
    }

    // Rollback degli step completati
    public async Task RollbackAsync(string conn)
    {
        while (_completati.Count > 0)
        {
            var step = _completati.Pop();
            Console.WriteLine($"↩️ Ripristino: {step.StepName}");
            await step.CompensateAsync(conn);
        }

        await Shell.Current.DisplayAlert("Procedura di ripristino!", "Db ripristinato ai dati iniziali.", "OK");
    }
}


public partial class ProteggiTabelleSagaStep : ISagaStep
{
    private readonly string _tabella;
    public string StepName => $"Backup tabella [{_tabella}]";
    public ProteggiTabelleSagaStep(string nomeTabella)
    {            
        _tabella = nomeTabella;
    }
    public async Task<bool> ExecuteAsync(string conn, MySqlTransaction? trans)
    {
        var dropQuery = $@"DROP TABLE IF EXISTS {_tabella}_shw;";
        bool ok = await SqlAsync.SqlNoQryString(conn, dropQuery);
        if (!ok) return false;

        var renameQuery = $"RENAME TABLE {_tabella} TO {_tabella}_shw;";                        
        ok = await SqlAsync.SqlNoQryString(conn, renameQuery);
        if (!ok) return false;

        return ok;
    }
    public async Task CompensateAsync(string conn)
    {
        var dropQuery = $@"DROP TABLE IF EXISTS {_tabella};";
        await SqlAsync.SqlNoQryString(conn, dropQuery);
        await Task.Delay(1000);

        var renameQuery = $"RENAME TABLE {_tabella}_shw TO {_tabella};";
        await SqlAsync.SqlNoQryString(conn, renameQuery);
        await Task.Delay(1000);
    }
}
