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
    
    // Recupera tutti gli elementi di un tipo specifico
   
    public async Task<List<T>> GetAllAsync<T>() where T : new()
    {
        await EnsureInitializedAsync();
        return await _database.Table<T>().ToListAsync();
    }
    
    // Recupera un elemento per ID
    // 
    public async Task<T?> GetByIdAsync<T>(int id) where T : class, new()
    {
        await EnsureInitializedAsync();
        return await _database.FindAsync<T>(id);
    }

    // Salva un elemento (Insert se nuovo, Update se esistente)
    
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
    
    /// Salva una lista di elementi
   
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
    
    /// Elimina un elemento
   
    public async Task<int> DeleteAsync<T>(T entity) where T : class
    {
        await EnsureInitializedAsync();
        return await _database.DeleteAsync(entity);
    }
        
    /// Elimina tutti gli elementi di un tipo specifico
    
    public async Task<int> DeleteAllAsync<T>() where T : new()
    {
        await EnsureInitializedAsync();
        return await _database.DeleteAllAsync<T>();
    }
        
    public async Task<Utente?> GetUtenteByNameAsync(string nomeUtente)
    {
        await EnsureInitializedAsync();
        return await _database.Table<Utente>()
            .Where(u => u.NomeUtente == nomeUtente)
            .FirstOrDefaultAsync();
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
        await DeleteAllAsync<Utente>();
        await DeleteAllAsync<OrigineConfig>();
        await DeleteAllAsync<NormalizzaConfig>();
        await DeleteAllAsync<ProgressConfig>();
        await DeleteAllAsync<FinalizzaConfig>();
        await DeleteAllAsync<FileSetupConfig>();
    }

    public string GetDatabasePath() => _dbPath;
}
