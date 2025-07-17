using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PvPmo.Services
{
    public class ServizioAutenticazione
    {
        private readonly List<Utente> _utenti;

        public ServizioAutenticazione()
        {
            _utenti = CaricaUtentiDaEmbedded("utenti.json");
        }

        public Utente? Autentica(string nomeInserito, string? password)
        {
            var utente = _utenti.FirstOrDefault(u =>
                u.NomeUtente.Equals(nomeInserito, StringComparison.OrdinalIgnoreCase));

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

        private static List<Utente> CaricaUtentiDaEmbedded(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(r => r.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

            if (resourceName is null)
                throw new FileNotFoundException($"Risorsa embedded '{fileName}' non trovata.");

            using var stream = assembly.GetManifestResourceStream(resourceName)!;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false) }
            };

            return JsonSerializer.Deserialize<List<Utente>>(stream, options) ?? new();
        }
    }
}
