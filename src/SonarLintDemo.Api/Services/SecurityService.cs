using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

namespace SonarLintDemo.Api.Services;

public class SecurityService
{
    private readonly ILogger<SecurityService> _logger;

    // S2068: Hard-coded credentials are security-sensitive
    private const string ApiKey = "sk-live-51234567890abcdef";

    public SecurityService(ILogger<SecurityService> logger)
    {
        _logger = logger;
    }

    public bool Authenticate(string key)
    {
        return key == ApiKey;
    }

    // S2083: Path traversal
    public string ReadReport(string reportName)
    {
        var path = $"C:\\reports\\{reportName}";
        _logger.LogInformation("Reading report from {Path}", path);
        return path;
    }

    // S2076: Command injection
    public string ExecuteTool(string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ping.exe",
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output;
    }

    // S2077: SQL injection
    public List<string> FindUsers(SqliteConnection connection, string filter)
    {
        var command = connection.CreateCommand();
        command.CommandText = $"SELECT Name FROM Users WHERE Name LIKE '%{filter}%'";

        var results = new List<string>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            results.Add(reader.GetString(0));
        }

        return results;
    }

    // S4790: Hashing data is security-sensitive (SHA1 is weak)
    public string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA1.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
