using System.IO;
using System.Text;
using System.Text.Json;
using MASA.PasswordGenerator.Core.Models;

namespace MASA.PasswordGenerator.Infrastructure.Export;

public static class PasswordExportService
{
    public static async Task ExportToCsvAsync(string filePath, IEnumerable<PasswordResult> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Index,Password,Length,Strength,EntropyBits,CrackTimeEstimate,CreatedAt");

        int index = 1;
        foreach (var r in results)
        {
            sb.AppendLine($"{index},\"{EscapeCsv(r.Value)}\",{r.Length},\"{r.Strength.StrengthLabel}\",{r.Strength.EntropyBits:F1},\"{r.Strength.CrackTimeEstimate}\",\"{r.CreatedAt:yyyy-MM-dd HH:mm:ss}\"");
            index++;
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
    }

    public static async Task ExportToJsonAsync(string filePath, IEnumerable<PasswordResult> results)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(results, options);
        await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
    }

    public static async Task ExportToTxtAsync(string filePath, IEnumerable<PasswordResult> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# MASA Password Generator Export - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine(new string('-', 60));

        foreach (var r in results)
        {
            sb.AppendLine(r.Value);
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
    }

    private static string EscapeCsv(string text) => text.Replace("\"", "\"\"");
}
