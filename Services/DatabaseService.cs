namespace PvPmo.Services;

public class DatabaseService
{
    private readonly SQLiteAsyncConnection _database;
    private readonly string _dbPath;
    private bool _initialized = false;
    private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

    public DatabaseService()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "pvpmo.db");
        _database = new SQLiteAsyncConnection(_dbPath);
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized)
            return;

        await _initLock.WaitAsync();
        try
        {
            if (!_initialized)
            {
                await _database.CreateTableAsync<Utente>();
                await _database.CreateTableAsync<OrigineConfig>();
                await _database.CreateTableAsync<NormalizzaConfig>();
                await _database.CreateTableAsync<ProgressConfig>();
                await _database.CreateTableAsync<FinalizzaConfig>();
                await _database.CreateTableAsync<FileSetupConfig>();

                // Seed degli utenti di default al primo avvio
                await SeedUtentiIfEmptyAsync();

                _initialized = true;
            }
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task SeedUtentiIfEmptyAsync()
    {
        var utentiCount = await _database.Table<Utente>().CountAsync();
        if (utentiCount > 0)
            return; // Database già popolato

        var utentiDefault = new List<Utente>
        {
            new Utente
            {
                NomeUtente = "admin",
                PasswordHash = "5994471abb01112afcc18159f6cc74b4f511b99806da59b3caf5a9c173cacfc5", // password: "12345"
                Ruolo = "Amministratore",
                Tipo = TipoUtente.Locale
            },
            new Utente
            {
                NomeUtente = "DL\\RCarlucci",
                Ruolo = "Amministratore",
                Tipo = TipoUtente.Windows
            },
            new Utente
            {
                NomeUtente = "DESKTOP-2TLJ8AM\\roby",
                Ruolo = "Amministratore",
                Tipo = TipoUtente.Windows
            }
        };

        foreach (var utente in utentiDefault)
        {
            await _database.InsertAsync(utente);
        }
    }

    // --- UTENTI ---
    public async Task<List<Utente>> GetUtentiAsync()
    {
        await EnsureInitializedAsync();
        return await _database.Table<Utente>().ToListAsync();
    }

    public async Task<Utente?> GetUtenteByNameAsync(string nomeUtente)
    {
        await EnsureInitializedAsync();
        return await _database.Table<Utente>()
            .Where(u => u.NomeUtente == nomeUtente)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveUtenteAsync(Utente utente)
    {
        await EnsureInitializedAsync();
        if (utente.Id != 0)
            return await _database.UpdateAsync(utente);
        return await _database.InsertAsync(utente);
    }

    public async Task<int> DeleteUtenteAsync(Utente utente)
    {
        await EnsureInitializedAsync();
        return await _database.DeleteAsync(utente);
    }

    // --- ORIGINE CONFIG ---
    public async Task<List<OrigineConfig>> GetOrigineConfigsAsync()
    {
        await EnsureInitializedAsync();
        return await _database.Table<OrigineConfig>().ToListAsync();
    }

    public async Task<int> SaveOrigineConfigAsync(OrigineConfig config)
    {
        await EnsureInitializedAsync();
        if (config.Id != 0)
            return await _database.UpdateAsync(config);
        return await _database.InsertAsync(config);
    }

    public async Task<int> DeleteOrigineConfigAsync(OrigineConfig config)
    {
        await EnsureInitializedAsync();
        return await _database.DeleteAsync(config);
    }

    // --- NORMALIZZA CONFIG ---
    public async Task<List<NormalizzaConfig>> GetNormalizzaConfigsAsync()
    {
        await EnsureInitializedAsync();
        return await _database.Table<NormalizzaConfig>().ToListAsync();
    }

    public async Task<int> SaveNormalizzaConfigAsync(NormalizzaConfig config)
    {
        await EnsureInitializedAsync();
        if (config.Id != 0)
            return await _database.UpdateAsync(config);
        return await _database.InsertAsync(config);
    }

    public async Task<int> DeleteNormalizzaConfigAsync(NormalizzaConfig config)
    {
        await EnsureInitializedAsync();
        return await _database.DeleteAsync(config);
    }

    // --- PROGRESS CONFIG ---
    public async Task<List<ProgressConfig>> GetProgressConfigsAsync()
    {
        await EnsureInitializedAsync();
        return await _database.Table<ProgressConfig>().ToListAsync();
    }

    public async Task<int> SaveProgressConfigAsync(ProgressConfig config)
    {
        await EnsureInitializedAsync();
        if (config.Id != 0)
            return await _database.UpdateAsync(config);
        return await _database.InsertAsync(config);
    }

    public async Task<int> DeleteProgressConfigAsync(ProgressConfig config)
    {
        await EnsureInitializedAsync();
        return await _database.DeleteAsync(config);
    }

    // --- FINALIZZA CONFIG ---
    public async Task<List<FinalizzaConfig>> GetFinalizzaConfigsAsync()
    {
        await EnsureInitializedAsync();
        return await _database.Table<FinalizzaConfig>().ToListAsync();
    }

    public async Task<int> SaveFinalizzaConfigAsync(FinalizzaConfig config)
    {
        await EnsureInitializedAsync();
        if (config.Id != 0)
            return await _database.UpdateAsync(config);
        return await _database.InsertAsync(config);
    }

    public async Task<int> DeleteFinalizzaConfigAsync(FinalizzaConfig config)
    {
        await EnsureInitializedAsync();
        return await _database.DeleteAsync(config);
    }

    // --- FILE SETUP CONFIG ---
    public async Task<List<FileSetupConfig>> GetFileSetupConfigsAsync()
    {
        await EnsureInitializedAsync();
        return await _database.Table<FileSetupConfig>().ToListAsync();
    }

    public async Task<FileSetupConfig?> GetFileSetupConfigByIdAsync(int id)
    {
        await EnsureInitializedAsync();
        return await _database.Table<FileSetupConfig>()
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveFileSetupConfigAsync(FileSetupConfig config)
    {
        await EnsureInitializedAsync();
        if (config.Id != 0)
            return await _database.UpdateAsync(config);
        return await _database.InsertAsync(config);
    }

    public async Task<int> DeleteFileSetupConfigAsync(FileSetupConfig config)
    {
        await EnsureInitializedAsync();
        return await _database.DeleteAsync(config);
    }

    public async Task<int> SaveAllFileSetupConfigsAsync(List<FileSetupConfig> configs)
    {
        await EnsureInitializedAsync();
        int count = 0;
        foreach (var config in configs)
        {
            count += await SaveFileSetupConfigAsync(config);
        }
        return count;
    }

    // --- UTILITY ---
    public async Task<bool> IsDatabaseEmptyAsync()
    {
        await EnsureInitializedAsync();
        var utentiCount = await _database.Table<Utente>().CountAsync();
        return utentiCount == 0;
    }

    public async Task ClearAllTablesAsync()
    {
        await EnsureInitializedAsync();
        await _database.DeleteAllAsync<Utente>();
        await _database.DeleteAllAsync<OrigineConfig>();
        await _database.DeleteAllAsync<NormalizzaConfig>();
        await _database.DeleteAllAsync<ProgressConfig>();
        await _database.DeleteAllAsync<FinalizzaConfig>();
        await _database.DeleteAllAsync<FileSetupConfig>();
    }

    public string GetDatabasePath() => _dbPath;
}
