using Microsoft.AspNetCore.Mvc;

namespace SonarLintDemo.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class NewCodeController : ControllerBase
{
    private readonly ILogger<NewCodeController> _logger;

    public NewCodeController(ILogger<NewCodeController> logger)
    {
        _logger = logger;
    }

    // Baseline: metodo simple con un code smell menor
    [HttpGet("baseline")]
    public IActionResult BaselineMethod(string name)
    {
        // S1135: Complete the task associated to this 'TODO' comment.
        // TODO: implementar validacion de entrada
        return Ok($"Hello, {name}");
    }

    // Baseline: magic numbers
    [HttpPost("calculate")]
    public IActionResult CalculateDiscount(decimal amount)
    {
        // S109: Assign this magic number to a named constant.
        var discount = amount * 0.15m;
        return Ok(discount);
    }

    // Baseline: metodo con complejidad cognitiva alta
    [HttpGet("grade")]
    public string GetGrade(int score)
    {
        if (score >= 90) return "A";
        if (score >= 80) return "B";
        if (score >= 70) return "C";
        if (score >= 60) return "D";
        if (score >= 0) return "F";
        return "Invalid";
    }
}
