using System.Text;
using System.Security.Cryptography;
using System.Text.Json;

namespace PvPmo.Services
{
    public class ServizioAutenticazione
    {
        private List<Utente> _utenti;

        public ServizioAutenticazione()
        {
            var json = File.ReadAllText("Resources/Config/utenti.json");
            _utenti = JsonSerializer.Deserialize<List<Utente>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false) }
            }) ?? new();

            //_utenti = JsonSerializer.Deserialize<List<Utente>>(json) ?? new();
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

            // Tipo Windows: confronta con il nome dell’utente attualmente loggato
            var utenteCorrente = $"{Environment.UserDomainName}\\{Environment.UserName}";
            return utente.NomeUtente.Equals(utenteCorrente, StringComparison.OrdinalIgnoreCase) ? utente : null;
        }

        //public Utente? Autentica(string nome, string? password)
        //{
        //    var utente = _utenti.FirstOrDefault(u => u.NomeUtente.Equals(nome, StringComparison.OrdinalIgnoreCase));
        //    if (utente is null) return null;

        //    if (utente.Tipo == TipoUtente.Locale)
        //    {
        //        if (string.IsNullOrWhiteSpace(password)) return null;
        //        var hash = CalcolaHash(password);
        //        return hash == utente.PasswordHash ? utente : null;
        //    }

        //    // Verifica utente Windows (senza password)
        //    if (utente.Tipo == TipoUtente.Windows)
        //    {
        //        var utenteCorrente = $"{Environment.UserDomainName}\\{Environment.UserName}";
        //        return nomeInserito.Equals(utenteCorrente, StringComparison.OrdinalIgnoreCase) ? utente : null;
        //    }

        //}

        private string CalcolaHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLower();
        }
    }

}
