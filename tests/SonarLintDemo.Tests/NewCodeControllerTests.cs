using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SonarLintDemo.Tests;

public class NewCodeControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public NewCodeControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Baseline_ReturnsGreeting()
    {
        var response = await _client.GetAsync("/newcode/baseline?name=Sonar");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Sonar", content);
    }

    [Theory]
    [InlineData(100, "A")]
    [InlineData(85, "B")]
    [InlineData(75, "C")]
    [InlineData(65, "D")]
    [InlineData(50, "F")]
    public async Task Grade_ReturnsExpectedGrade(int score, string expected)
    {
        var response = await _client.GetAsync($"/newcode/grade?score={score}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(expected, content);
    }

    [Fact]
    public async Task CalculateDiscount_ReturnsDiscountedAmount()
    {
        var response = await _client.PostAsync("/newcode/calculate?amount=100", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("15", content);
    }
}
