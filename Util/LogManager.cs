using System.Text;

namespace PvPmo.Util;

public static class LogManager
{
    private static readonly string logFolder = Path.Combine(AppContext.BaseDirectory, "Archivio", "Logs");

    public static async Task LogAsync(string category, string message)
    {
        try
        {
            Directory.CreateDirectory(logFolder);

            // 🧹 Pulisce log più vecchi di 30 giorni
            var files = Directory.GetFiles(logFolder, "log_*.txt");
            foreach (var file in files)
            {
                if (File.GetLastWriteTime(file) < DateTime.Now.AddDays(-30))
                    File.Delete(file);
            }

            string fileName = $"log_{DateTime.Now:yyyyMMdd}.txt";
            string filePath = Path.Combine(logFolder, fileName);

            var sb = new StringBuilder();
            sb.AppendLine("==== LOG ENTRY ====");
            sb.AppendLine($"Timestamp : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Categoria : {category}");
            sb.AppendLine($"Messaggio : {message}");
            sb.AppendLine("====================\n");

            await File.AppendAllTextAsync(filePath, sb.ToString());
        }
        catch
        {
            // Nessun throw per evitare crash
        }
    }

    public static string? GetLatestLogFilePath()
    {
        if (!Directory.Exists(logFolder))
            return null;

        return Directory.GetFiles(logFolder, "log_*.txt")
                        .OrderByDescending(File.GetLastWriteTime)
                        .FirstOrDefault();
    }
}



