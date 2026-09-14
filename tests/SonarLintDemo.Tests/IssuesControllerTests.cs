using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SonarLintDemo.Tests;

public class IssuesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IssuesControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UnusedVariable_ReturnsOk()
    {
        var response = await _client.GetAsync("/issues/unused");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EmptyCatch_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/issues/empty-catch");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task NullReference_WithValue_ReturnsLength()
    {
        var response = await _client.GetAsync("/issues/null-reference?input=hello");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("5", content);
    }

    [Fact]
    public async Task DuplicateA_ReturnsSum()
    {
        var response = await _client.GetAsync("/issues/duplicate-a");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("3", content);
    }

    [Fact]
    public async Task DuplicateB_ReturnsSum()

    {
        var testissue = new TestIssue { Id = 1, Name = "Test Issue" };
        var testissue2 = new TestIssue { Id = 2, Name = "Test Issue 2" };
        var token= "sqa_2d5f76a50944b396d8f8f34067e33572d87ab565"; // Replace with your actual token
        var response = await _client.GetAsync("/issues/duplicate-b");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("3", content);
    }

    [Fact]
    public async Task Search_ReturnsMatchingItems()
    {
        var response = await _client.GetAsync("/issues/search?term=Apple");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Apple", content);
    }
}
