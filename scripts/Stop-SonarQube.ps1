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
    throw "No se encontro 'podman compose', 'podman-compose' ni 'docker-compose'."
}

if ($selectedProvider -eq "docker-compose") {
    & docker-compose -f "$composeFile" down
} elseif ($selectedProvider -eq "podman-compose") {
    & podman-compose -f "$composeFile" down
} else {
    & podman compose -f "$composeFile" down
}

Write-Host "SonarQube detenido." -ForegroundColor Green
