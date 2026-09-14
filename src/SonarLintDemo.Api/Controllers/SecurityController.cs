using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Mvc;

namespace SonarLintDemo.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class SecurityController : ControllerBase
{
    private readonly ILogger<SecurityController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    // S2068: Credentials should not be hard-coded
    private const string AdminPassword = "P@ssw0rd123!";

    public SecurityController(ILogger<SecurityController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("login")]
    public IActionResult Login(string username, string password)
    {
        // S2068: Hardcoded password
        if (username == "admin" && password == AdminPassword)
        {
            return Ok("Authenticated");
        }

        return Unauthorized();
    }

    [HttpGet("read-file")]
    public IActionResult ReadFile(string filename)
    {
        // S2083: Path traversal
        var path = $"C:\\app\\data\\{filename}";
        _logger.LogInformation("Reading file: {Path}", path);
        return Ok($"Would read: {path}");
    }

    [HttpGet("run")]
    public IActionResult RunCommand(string command)
    {
        // S2076: Command injection
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                UseShellExecute = false
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return Ok(output);
    }

    [HttpGet("search-xml")]
    public IActionResult SearchXml(string username)
    {
        // S2091: XPath injection
        var xml = @"<users><user name='admin' role='admin'/><user name='guest' role='user'/></users>";
        var doc = new XmlDocument();
        doc.LoadXml(xml);

        var xpath = $"/users/user[@name='{username}']";
        var node = doc.SelectSingleNode(xpath);

        return Ok(node?.OuterXml ?? "Not found");
    }

    [HttpGet("echo")]
    public ContentResult Echo(string message)
    {
        // S5131: XSS - user input reflected without encoding
        return Content($"<html><body><h1>{message}</h1></body></html>", "text/html");
    }

    [HttpGet("redirect")]
    public IActionResult RedirectUser(string url)
    {
        // S5144: Open redirect
        return Redirect(url);
    }

    [HttpGet("fetch")]
    public async Task<IActionResult> FetchUrl(string url)
    {
        // S5334: SSRF - Server-Side Request Forgery
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetStringAsync(url);
        return Ok(response);
    }

    [HttpGet("hash")]
    public IActionResult HashPassword(string password)
    {
        // S4426: Weak cryptography (MD5)
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = MD5.HashData(bytes);
        return Ok(Convert.ToHexString(hash));
    }

    [HttpGet("cookie")]
    public IActionResult SetAuthCookie()
    {
        // S2092 / S4787: Insecure cookie (no HttpOnly, no Secure, no SameSite)
        Response.Cookies.Append("AuthToken", "secret-value", new CookieOptions
        {
            HttpOnly = false,
            Secure = false,
            SameSite = SameSiteMode.None
        });

        return Ok("Cookie set");
    }
}
