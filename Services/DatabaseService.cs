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

    // --- METODI CRUD GENERICI ---

    /// <summary>
    /// Recupera tutti gli elementi di un tipo specifico
    /// </summary>
    public async Task<List<T>> GetAllAsync<T>() where T : new()
    {
        await EnsureInitializedAsync();
        return await _database.Table<T>().ToListAsync();
    }

    /// <summary>
    /// Recupera un elemento per ID
    /// </summary>
    public async Task<T?> GetByIdAsync<T>(int id) where T : class, new()
    {
        await EnsureInitializedAsync();
        return await _database.FindAsync<T>(id);
    }

    /// <summary>
    /// Salva un elemento (Insert se nuovo, Update se esistente)
    /// </summary>
    public async Task<int> SaveAsync<T>(T entity) where T : class
    {
        await EnsureInitializedAsync();

        // Controlla se l'entità ha una proprietà Id con valore != 0
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty != null)
        {
            var idValue = idProperty.GetValue(entity);
            if (idValue is int id && id != 0)
                return await _database.UpdateAsync(entity);
        }

        return await _database.InsertAsync(entity);
    }

    /// <summary>
    /// Salva una lista di elementi
    /// </summary>
    public async Task<int> SaveAllAsync<T>(List<T> entities) where T : class
    {
        await EnsureInitializedAsync();
        int count = 0;
        foreach (var entity in entities)
        {
            count += await SaveAsync(entity);
        }
        return count;
    }

    /// <summary>
    /// Elimina un elemento
    /// </summary>
    public async Task<int> DeleteAsync<T>(T entity) where T : class
    {
        await EnsureInitializedAsync();
        return await _database.DeleteAsync(entity);
    }

    /// <summary>
    /// Elimina tutti gli elementi di un tipo specifico
    /// </summary>
    public async Task<int> DeleteAllAsync<T>() where T : new()
    {
        await EnsureInitializedAsync();
        return await _database.DeleteAllAsync<T>();
    }

    // --- METODI LEGACY (per retrocompatibilità) ---
    // Questi metodi chiamano i metodi generici e possono essere rimossi in futuro

    public async Task<List<Utente>> GetUtentiAsync() => await GetAllAsync<Utente>();
    public async Task<int> SaveUtenteAsync(Utente utente) => await SaveAsync(utente);
    public async Task<int> DeleteUtenteAsync(Utente utente) => await DeleteAsync(utente);

    public async Task<Utente?> GetUtenteByNameAsync(string nomeUtente)
    {
        await EnsureInitializedAsync();
        return await _database.Table<Utente>()
            .Where(u => u.NomeUtente == nomeUtente)
            .FirstOrDefaultAsync();
    }

    public async Task<List<OrigineConfig>> GetOrigineConfigsAsync() => await GetAllAsync<OrigineConfig>();
    public async Task<int> SaveOrigineConfigAsync(OrigineConfig config) => await SaveAsync(config);
    public async Task<int> DeleteOrigineConfigAsync(OrigineConfig config) => await DeleteAsync(config);

    public async Task<List<NormalizzaConfig>> GetNormalizzaConfigsAsync() => await GetAllAsync<NormalizzaConfig>();
    public async Task<int> SaveNormalizzaConfigAsync(NormalizzaConfig config) => await SaveAsync(config);
    public async Task<int> DeleteNormalizzaConfigAsync(NormalizzaConfig config) => await DeleteAsync(config);

    public async Task<List<ProgressConfig>> GetProgressConfigsAsync() => await GetAllAsync<ProgressConfig>();
    public async Task<int> SaveProgressConfigAsync(ProgressConfig config) => await SaveAsync(config);
    public async Task<int> DeleteProgressConfigAsync(ProgressConfig config) => await DeleteAsync(config);

    public async Task<List<FinalizzaConfig>> GetFinalizzaConfigsAsync() => await GetAllAsync<FinalizzaConfig>();
    public async Task<int> SaveFinalizzaConfigAsync(FinalizzaConfig config) => await SaveAsync(config);
    public async Task<int> DeleteFinalizzaConfigAsync(FinalizzaConfig config) => await DeleteAsync(config);

    public async Task<List<FileSetupConfig>> GetFileSetupConfigsAsync() => await GetAllAsync<FileSetupConfig>();
    public async Task<FileSetupConfig?> GetFileSetupConfigByIdAsync(int id) => await GetByIdAsync<FileSetupConfig>(id);
    public async Task<int> SaveFileSetupConfigAsync(FileSetupConfig config) => await SaveAsync(config);
    public async Task<int> DeleteFileSetupConfigAsync(FileSetupConfig config) => await DeleteAsync(config);
    public async Task<int> SaveAllFileSetupConfigsAsync(List<FileSetupConfig> configs) => await SaveAllAsync(configs);

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
        await DeleteAllAsync<Utente>();
        await DeleteAllAsync<OrigineConfig>();
        await DeleteAllAsync<NormalizzaConfig>();
        await DeleteAllAsync<ProgressConfig>();
        await DeleteAllAsync<FinalizzaConfig>();
        await DeleteAllAsync<FileSetupConfig>();
    }

    public string GetDatabasePath() => _dbPath;
}
