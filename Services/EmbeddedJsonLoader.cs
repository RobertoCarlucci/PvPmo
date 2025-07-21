using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PvPmo.Services
{
    public static class EmbeddedJsonLoader
    {
        public static async Task<List<T>> LoadJsonAsync<T>(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fullName = assembly.GetManifestResourceNames()
                                   .FirstOrDefault(r => r.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));

            if (fullName is null)
                throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");

            using Stream stream = assembly.GetManifestResourceStream(fullName)!;
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            //var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return await JsonSerializer.DeserializeAsync<List<T>>(stream, options) ?? new List<T>();
        }
    }
}
