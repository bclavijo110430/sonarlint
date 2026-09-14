[CmdletBinding()]
param(
    [string]$SonarHostUrl = "http://localhost:9000",
    [string]$ProjectKey = "SonarLintDemo",
    [string]$ProjectName = "SonarLint Demo",
    [string]$AdminUser = "admin",
    [string]$AdminPassword = "admin"
)

$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..")

function Test-CommandAvailable {
    param([string]$Command)
    return [bool](Get-Command $Command -ErrorAction SilentlyContinue)
}

Write-Host "=== Inicializando entorno SonarLint + SonarQube ===" -ForegroundColor Cyan

# 1. Verificar herramientas
Write-Host "`nVerificando herramientas .NET..." -ForegroundColor Cyan

if (-not (Test-CommandAvailable "dotnet")) {
    throw "dotnet CLI no esta instalado o no esta en el PATH."
}

$tools = @("dotnet-sonarscanner", "dotnet-coverage")
foreach ($tool in $tools) {
    $installed = & dotnet tool list --global | Select-String $tool
    if (-not $installed) {
        Write-Host "Instalando $tool..." -ForegroundColor Yellow
        & dotnet tool install --global $tool
        if ($LASTEXITCODE -ne 0) { throw "Error instalando $tool" }
    } else {
        Write-Host "$tool ya esta instalado." -ForegroundColor Green
    }
}

# 2. Restaurar paquetes NuGet
Write-Host "`nRestaurando paquetes NuGet..." -ForegroundColor Cyan
& dotnet restore "$root\SonarLintDemo.sln"
if ($LASTEXITCODE -ne 0) { throw "Error en dotnet restore" }

# 3. Verificar que SonarQube responde
Write-Host "`nVerificando conexion con SonarQube en $SonarHostUrl..." -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "$SonarHostUrl/api/system/status" -Method GET -TimeoutSec 10
    Write-Host "SonarQube responde. Version: $($response.version), Status: $($response.status)" -ForegroundColor Green
}
catch {
    throw "No se pudo conectar con SonarQube en $SonarHostUrl. Asegurate de que este ejecutandose."
}

# 4. Crear proyecto en SonarQube
Write-Host "`nCreando proyecto '$ProjectKey' en SonarQube..." -ForegroundColor Cyan
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${AdminUser}:${AdminPassword}"))
$headers = @{
    Authorization = "Basic $auth"
}

$projectExists = $false
try {
    $existing = Invoke-RestMethod -Uri "$SonarHostUrl/api/projects/search?projects=$ProjectKey" -Headers $headers -Method GET
    $projectExists = $existing.components.Count -gt 0
}
catch {
    Write-Host "No se pudo verificar si el proyecto existe. Continuando..." -ForegroundColor Yellow
}

if (-not $projectExists) {
    $body = @{
        project = $ProjectKey
        name = $ProjectName
        visibility = "private"
    }
    try {
        Invoke-RestMethod -Uri "$SonarHostUrl/api/projects/create" -Headers $headers -Method POST -Body $body | Out-Null
        Write-Host "Proyecto creado correctamente." -ForegroundColor Green
    }
    catch {
        Write-Host "Error creando proyecto (puede que ya exista): $_" -ForegroundColor Yellow
    }
} else {
    Write-Host "El proyecto ya existe en SonarQube." -ForegroundColor Green
}

# 5. Generar token de usuario
Write-Host "`nGenerando token de usuario para analisis..." -ForegroundColor Cyan
$tokenName = "sonarlint-demo-token-$(Get-Date -Format yyyyMMddHHmmss)"
$tokenBody = @{
    name = $tokenName
    type = "USER_TOKEN"
}

try {
    $tokenResponse = Invoke-RestMethod -Uri "$SonarHostUrl/api/user_tokens/generate" -Headers $headers -Method POST -Body $tokenBody
    $token = $tokenResponse.token
    Write-Host "`n=========================================" -ForegroundColor Green
    Write-Host "TOKEN GENERADO (guardalo en un lugar seguro):" -ForegroundColor Green
    Write-Host $token -ForegroundColor Yellow
    Write-Host "=========================================" -ForegroundColor Green
    Write-Host "`nUsa este token para ejecutar:" -ForegroundColor Cyan
    Write-Host ".\scripts\Start-Analysis.ps1 -SonarToken '$token'" -ForegroundColor White
}
catch {
    Write-Host "Error generando token: $_" -ForegroundColor Red
    Write-Host "Puedes generarlo manualmente desde: $SonarHostUrl/account/security" -ForegroundColor Yellow
}

Write-Host "`nInicializacion completada." -ForegroundColor Green
