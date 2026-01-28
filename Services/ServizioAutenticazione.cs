using System.Security.Cryptography;
using System.Text;

namespace PvPmo.Services;

public class ServizioAutenticazione
{
    private readonly DatabaseService _databaseService;

    public ServizioAutenticazione(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<Utente?> AutenticaAsync(string nomeInserito, string? password)
    {
        var utente = await _databaseService.GetUtenteByNameAsync(nomeInserito);

        if (utente is null) return null;

        if (utente.Tipo == TipoUtente.Locale)
        {
            if (string.IsNullOrWhiteSpace(password)) return null;
            return CalcolaHash(password) == utente.PasswordHash ? utente : null;
        }

        var utenteCorrente = $"{Environment.UserDomainName}\\{Environment.UserName}";
        return utente.NomeUtente.Equals(utenteCorrente, StringComparison.OrdinalIgnoreCase) ? utente : null;
    }

    private static string CalcolaHash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLower();
    }
}
