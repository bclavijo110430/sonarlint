[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$SonarToken,

    [string]$SonarHostUrl = "http://localhost:9000",
    [string]$ProjectKey = "SonarLintDemo",
    [string]$ProjectName = "SonarLint Demo"
)

$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$coverageDir = Join-Path $root "TestResults"
$coverageFile = Join-Path $coverageDir "coverage.xml"

if (-not (Test-Path $coverageDir)) {
    New-Item -ItemType Directory -Path $coverageDir | Out-Null
}

Write-Host "Iniciando analisis con SonarScanner for .NET..." -ForegroundColor Cyan

& dotnet sonarscanner begin `
    /k:"$ProjectKey" `
    /n:"$ProjectName" `
    /d:sonar.host.url="$SonarHostUrl" `
    /d:sonar.token="$SonarToken" `
    /d:sonar.cs.vscoveragexml.reportsPaths="$coverageFile"

if ($LASTEXITCODE -ne 0) { throw "Error en dotnet sonarscanner begin" }

try {
    Write-Host "Compilando solucion..." -ForegroundColor Cyan
    & dotnet build "$root\SonarLintDemo.sln" --no-incremental
    if ($LASTEXITCODE -ne 0) { throw "Error en dotnet build" }

    Write-Host "Ejecutando tests con cobertura..." -ForegroundColor Cyan
    & dotnet-coverage collect "dotnet test $root\SonarLintDemo.sln" `
        -f xml `
        -o "$coverageFile"
    if ($LASTEXITCODE -ne 0) { throw "Error en dotnet test / dotnet-coverage" }

    Write-Host "Finalizando analisis y enviando a SonarQube..." -ForegroundColor Cyan
    & dotnet sonarscanner end /d:sonar.token="$SonarToken"
    if ($LASTEXITCODE -ne 0) { throw "Error en dotnet sonarscanner end" }
}
catch {
    Write-Host "Ocurrio un error durante el analisis: $_" -ForegroundColor Red
    throw
}

Write-Host "`nAnalisis completado. Revisa los resultados en: $SonarHostUrl/dashboard?id=$ProjectKey" -ForegroundColor Green
