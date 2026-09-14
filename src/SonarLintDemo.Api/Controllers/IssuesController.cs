using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace SonarLintDemo.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class IssuesController : ControllerBase
{
    private readonly ILogger<IssuesController> _logger;

    public IssuesController(ILogger<IssuesController> logger)
    {
        _logger = logger;
    }

    // S1481: Variable no utilizada
    [HttpGet("unused")]
    public IActionResult UnusedVariable()
    {
        int unusedValue = 42;
        return Ok("OK");
    }

    // S108: Bloque catch vacio
    [HttpGet("empty-catch")]
    public IActionResult EmptyCatch()
    {
        try
        {
            int result = 10 / int.Parse("0");
            return Ok(result);
        }
        catch
        {
        }

        return BadRequest();
    }

    // S2259: Posible referencia nula
    [HttpGet("null-reference")]
    public IActionResult NullReference(string? input)
    {
        var length = input.Length;
        return Ok(length);
    }

    // Codigo duplicado intencional (S1192 / S4144)
    [HttpGet("duplicate-a")]
    public IActionResult DuplicateA()
    {
        var x = 1;
        var y = 2;
        var z = x + y;
        return Ok(z);
    }

    [HttpGet("duplicate-b")]
    public IActionResult DuplicateB()
    {
        var x = 1;
        var y = 2;
        var z = x + y;
        return Ok(z);
    }

    // S3649: Posible inyeccion SQL
    [HttpGet("search")]
    public IActionResult Search([FromServices] SqliteConnection connection, string term)
    {
        var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM Items WHERE Name = '{term}'";
        var reader = command.ExecuteReader();

        var results = new List<string>();
        while (reader.Read())
        {
            results.Add(reader.GetString(1));
        }

        return Ok(results);
    }
}
