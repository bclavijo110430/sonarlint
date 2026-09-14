[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$controllerPath = Join-Path (Join-Path (Join-Path $PSScriptRoot "..") "src") "SonarLintDemo.Api\Controllers\NewCodeController.cs"

if (-not (Test-Path $controllerPath)) {
    throw "No se encontro el archivo: $controllerPath"
}
$test=qeqw
$additionalCode = @"

    // ============================================================
    // NEW CODE: agregado despues del analisis inicial (baseline)
    // Estos metodos se detectaran como "new code" en SonarQube.
    // ============================================================

    // S3963: Initialize all static fields inline and remove the static constructor.
    private static readonly DateTime AppStartDate;

    static NewCodeController()
    {
        AppStartDate = DateTime.UtcNow;
    }

    // S1854: Remove this useless assignment to local variable 'status'.
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        var status = "OK";
        status = CheckStatus();
        return Ok(status);
    }

    private string CheckStatus()
    {
        return "Running since " + AppStartDate;
    }

    // S2589: Change this condition so that it does not always evaluate to 'false'.
    [HttpGet("validate")]
    public IActionResult ValidateInput(string? input)
    {
        if (input is null && input is not null)
        {
            return BadRequest();
        }

        return Ok(input);
    }

    // S108: Nested blocks of code should not be left empty
    [HttpGet("noop")]
    public IActionResult NoOperation()
    {
        try
        {
            _logger.LogInformation("Operation started");
        }
        catch (Exception ex)
        {
            // TODO: log exception
        }

        return Ok();
    }
"@

$content = Get-Content -Path $controllerPath -Raw

# Insertar antes del ultimo cierre de llave de la clase
$lastBrace = $content.LastIndexOf('}')
if ($lastBrace -lt 0) {
    throw "No se pudo encontrar el cierre de la clase en $controllerPath"
}

$newContent = $content.Insert($lastBrace, $additionalCode)
Set-Content -Path $controllerPath -Value $newContent -NoNewline

Write-Host "Codigo nuevo agregado a NewCodeController.cs" -ForegroundColor Green
Write-Host "`nPasos siguientes:" -ForegroundColor Cyan
Write-Host "1. Ejecuta: .\scripts\Start-Analysis.ps1 -SonarToken <tu-token>" -ForegroundColor White
Write-Host "2. Abre SonarQube y revisa la pestana 'New Code'" -ForegroundColor White
Write-Host "3. Los metodos GetStatus, ValidateInput y NoOperation deberian aparecer como nuevos issues." -ForegroundColor White
