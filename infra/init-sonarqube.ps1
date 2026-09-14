[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

Write-Host "Configurando la maquina Podman para SonarQube..." -ForegroundColor Cyan
Write-Host "Este proceso configura vm.max_map_count dentro de la VM de Podman (WSL2)." -ForegroundColor Cyan

$machineStatus = & podman machine info --format json | ConvertFrom-Json | Select-Object -ExpandProperty Host

if (-not $machineStatus) {
    throw "No se pudo obtener informacion de la maquina Podman. Asegurate de tener Podman instalado y una maquina inicializada."
}

Write-Host "Aplicando vm.max_map_count=262144 en la maquina Podman..." -ForegroundColor Yellow
& podman machine ssh "sudo sysctl -w vm.max_map_count=262144"

if ($LASTEXITCODE -ne 0) {
    throw "No se pudo configurar vm.max_map_count. Si la maquina es rootless, prueba ejecutar: podman machine set --rootful"
}

Write-Host "Configuracion aplicada correctamente." -ForegroundColor Green
Write-Host "IMPORTANTE: si reinicias la maquina de Podman, ejecuta este script nuevamente." -ForegroundColor Yellow
