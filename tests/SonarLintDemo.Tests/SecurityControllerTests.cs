using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SonarLintDemo.Tests;

public class SecurityControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SecurityControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Login_WithHardcodedCredentials_ReturnsOk()
    {
        var response = await _client.GetAsync("/security/login?username=admin&password=P@ssw0rd123!");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Authenticated", content);
    }

    [Fact]
    public async Task ReadFile_ReturnsPathWithUserInput()
    {
        var response = await _client.GetAsync("/security/read-file?filename=report.txt");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("report.txt", content);
    }

    [Fact]
    public async Task Echo_ReturnsHtmlWithoutEncoding()
    {
        var response = await _client.GetAsync("/security/echo?message=<script>alert(1)</script>");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("<script>alert(1)</script>", content);
    }

    [Fact]
    public async Task Redirect_ReturnsRedirectToUserUrl()
    {
        var response = await _client.GetAsync("/security/redirect?url=https://example.com");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://example.com/", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Hash_ReturnsMd5Hash()
    {
        var response = await _client.GetAsync("/security/hash?password=hello");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(32, content.Length); // MD5 hex string length
    }

    [Fact]
    public async Task Cookie_SetsInsecureAuthCookie()
    {
        var response = await _client.GetAsync("/security/cookie");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var setCookie = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
        Assert.NotNull(setCookie);
        Assert.Contains("AuthToken", setCookie);
    }
}
