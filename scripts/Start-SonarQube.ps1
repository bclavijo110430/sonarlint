[CmdletBinding()]
param(
    [ValidateSet("auto", "podman", "podman-compose", "docker-compose")]
    [string]$ComposeProvider = "auto"
)

$ErrorActionPreference = "Stop"

function Test-CommandAvailable {
    param([string]$Command)
    return [bool](Get-Command $Command -ErrorAction SilentlyContinue)
}

function Test-PodmanComposeAvailable {
    try {
        & podman compose version | Out-Null
        return $LASTEXITCODE -eq 0
    }
    catch {
        return $false
    }
}

if (-not (Test-CommandAvailable "podman")) {
    throw "Podman no esta instalado o no esta en el PATH."
}

$infraDir = Join-Path (Join-Path $PSScriptRoot "..") "infra"
$composeFile = Join-Path $infraDir "compose.yml"

$selectedProvider = $null
if ($ComposeProvider -ne "auto") {
    if ($ComposeProvider -eq "podman") {
        if (-not (Test-PodmanComposeAvailable)) {
            throw "El subcomando 'podman compose' no esta disponible."
        }
        $selectedProvider = "podman"
    } elseif (-not (Test-CommandAvailable $ComposeProvider)) {
        throw "El proveedor especificado '$ComposeProvider' no esta disponible en el PATH."
    } else {
        $selectedProvider = $ComposeProvider
    }
} elseif (Test-PodmanComposeAvailable) {
    $selectedProvider = "podman"
} elseif (Test-CommandAvailable "podman-compose") {
    $selectedProvider = "podman-compose"
} elseif (Test-CommandAvailable "docker-compose") {
    $selectedProvider = "docker-compose"
} else {
    throw "No se encontro 'podman compose', 'podman-compose' ni 'docker-compose'. Instala alguno para continuar."
}

Write-Host "Usando proveedor de Compose: $selectedProvider" -ForegroundColor Cyan

if ($selectedProvider -eq "docker-compose") {
    & docker-compose -f "$composeFile" up -d
} elseif ($selectedProvider -eq "podman-compose") {
    & podman-compose -f "$composeFile" up -d
} else {
    & podman compose -f "$composeFile" up -d
}

if ($LASTEXITCODE -ne 0) {
    throw "Error al iniciar SonarQube. Revisa los logs con: podman logs sonarqube-community"
}

Write-Host "`nSonarQube esta iniciando. Espera 1-2 minutos y accede a: http://localhost:9000" -ForegroundColor Green
Write-Host "Credenciales por defecto: admin / admin" -ForegroundColor Yellow
Write-Host "`nPara ver logs: podman logs -f sonarqube-community" -ForegroundColor Cyan
