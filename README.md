# SonarLint + SonarQube Community Build + .NET

Este proyecto demuestra como integrar **SonarLint** en **Visual Studio Code** con un servidor local de **SonarQube Community Build** para analizar una aplicacion **ASP.NET Core Web API** escrita en **.NET 10**.

Incluye codigo con problemas intencionales para que puedas ver como SonarLint y SonarQube detectan issues de calidad y seguridad.

---

## Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download) o superior
- [Podman](https://podman.io/) para Windows (con maquina Podman inicializada)
- `podman-compose` o `docker-compose` instalado
- [Visual Studio Code](https://code.visualstudio.com/)
- Extension [SonarQube for VS Code](https://marketplace.visualstudio.com/items?itemName=SonarSource.sonarlint-vscode)

---

## Estructura del proyecto

```
.
├── infra/
│   ├── compose.yml              # SonarQube + PostgreSQL
│   └── init-sonarqube.ps1       # Configura vm.max_map_count en Podman
├── scripts/
│   ├── Start-SonarQube.ps1      # Levanta SonarQube
│   ├── Stop-SonarQube.ps1       # Detiene SonarQube
│   ├── Start-Analysis.ps1       # Ejecuta SonarScanner con cobertura
│   └── Initialize-Project.ps1   # Instala tools, crea proyecto y token
├── src/
│   └── SonarLintDemo.Api/       # Web API .NET 10 con issues de ejemplo
├── tests/
│   └── SonarLintDemo.Tests/     # Tests xUnit con Coverlet
├── .vscode/
│   ├── extensions.json          # Extensiones recomendadas
│   └── settings.json            # Configuracion compartida de SonarLint
├── .sonarlint/
│   └── connectedMode.json       # Binding compartido del proyecto
├── nuget.config                 # Usa solo nuget.org
└── README.md
```

---

## Inicio rapido

### 1. Configurar Podman

SonarQube requiere `vm.max_map_count >= 262144`. Ejecuta una sola vez:

```powershell
.\infra\init-sonarqube.ps1
```

> Si la maquina Podman es **rootless** y el script falla, configurala como rootful:
> ```powershell
> podman machine stop
> podman machine set --rootful
> podman machine start
> ```

### 2. Iniciar SonarQube Community Build

```powershell
.\scripts\Start-SonarQube.ps1
```

El script detecta automaticamente `podman compose` (subcomando nativo), `podman-compose` o `docker-compose`. Si prefieres forzar uno, usa:

```powershell
.\scripts\Start-SonarQube.ps1 -ComposeProvider podman          # subcomando nativo
.\scripts\Start-SonarQube.ps1 -ComposeProvider podman-compose  # script Python
.\scripts\Start-SonarQube.ps1 -ComposeProvider docker-compose  # Docker Compose
```

Espera 1-2 minutos y abre: [http://localhost:9000](http://localhost:9000)

Credenciales por defecto:
- Usuario: `admin`
- Contrasena: `admin`

Se te pedira cambiar la contrasena al iniciar sesion.

### 3. Inicializar el proyecto en SonarQube

```powershell
.\scripts\Initialize-Project.ps1 -AdminPassword <tu-nueva-contrasena>
```

Este script:
- Instala `dotnet-sonarscanner` y `dotnet-coverage`
- Restaura paquetes NuGet
- Crea el proyecto `SonarLintDemo` en SonarQube
- Genera un **User Token** para el analisis

Guarda el token generado, lo necesitaras en el siguiente paso.

### 4. Ejecutar el analisis con cobertura

```powershell
.\scripts\Start-Analysis.ps1 -SonarToken <tu-token>
```

Al finalizar, revisa los resultados en:
[http://localhost:9000/dashboard?id=SonarLintDemo](http://localhost:9000/dashboard?id=SonarLintDemo)

### 5. Configurar SonarLint en VS Code

1. Abre el proyecto en VS Code.
2. Instala la extension **SonarQube for VS Code** (usando `.vscode/extensions.json`).
3. Abre la vista **SONARQUBE SETUP > CONNECTED MODE** en la barra lateral.
4. Agrega una conexion a SonarQube Server:
   - **Connection ID**: `local-sonarqube`
   - **Server URL**: `http://localhost:9000`
   - **Token**: tu token de usuario
5. El proyecto ya tiene el binding compartido en `.sonarlint/connectedMode.json` y `.vscode/settings.json`. VS Code te solicitara confirmar el binding.

Una vez conectado, SonarLint resaltara los issues directamente en el editor.

---

## Problemas intencionales incluidos

El controlador `IssuesController` contiene varios issues tipicos que SonarQube/SonarLint detectaran:

| Endpoint | Issue esperado | Regla aproximada |
|---|---|---|
| `GET /issues/unused` | Variable asignada pero no usada | S1481 |
| `GET /issues/empty-catch` | Bloque `catch` vacio | S108 |
| `GET /issues/null-reference` | Desreferencia posiblemente nula | S2259 |
| `GET /issues/duplicate-a` / `duplicate-b` | Codigo duplicado | S1192 / S4144 |
| `GET /issues/search` | Posible inyeccion SQL | S3649 |

---

## Scripts disponibles

| Script | Descripcion |
|---|---|
| `infra/init-sonarqube.ps1` | Configura `vm.max_map_count` en la maquina Podman |
| `scripts/Start-SonarQube.ps1` | Levanta SonarQube y PostgreSQL |
| `scripts/Stop-SonarQube.ps1` | Detiene los contenedores. Acepta `-ComposeProvider` |
| `scripts/Initialize-Project.ps1` | Prepara tools .NET y proyecto en SonarQube |
| `scripts/Start-Analysis.ps1` | Ejecuta SonarScanner con cobertura de tests |

---

## Detener el entorno

```powershell
.\scripts\Stop-SonarQube.ps1
```

Los datos persistentes se guardan en volumenes de Podman, por lo que no se perderan al detener los contenedores.

---

## Solucion de problemas

### Los contenedores no inician

Verifica los logs:
```powershell
podman logs sonarqube-community
podman logs sonarqube-postgres
```

### Error de `vm.max_map_count`

Asegurate de ejecutar `infra/init-sonarqube.ps1`. Si persiste, configura la maquina Podman como rootful.

### Error de autenticacion de NuGet

El archivo `nuget.config` limita los origenes a `nuget.org`. Si tu entorno tiene feeds privados configurados globalmente, este archivo los ignora para este proyecto.

### SonarLint no se conecta

- Verifica que SonarQube este corriendo en `http://localhost:9000`.
- Asegurate de usar un **User Token**, no un Project Token ni Global Token.
- Revisa que el `connectionId` en tus User Settings de VS Code coincida con el de `.vscode/settings.json` (`local-sonarqube`).

---

## Limitaciones de SonarQube Community Build

- No incluye analisis de ramas ni pull requests.
- Algunas reglas de seguridad avanzadas estan limitadas respecto a las ediciones de pago.
- Para equipos grandes o proyectos productivos, considera **Developer Edition** o superior.

---

## Referencias

- [SonarQube Community Build](https://docs.sonarsource.com/sonarqube-community-build/)
- [SonarQube for VS Code](https://docs.sonarsource.com/sonarqube-for-vs-code/)
- [SonarScanner for .NET](https://docs.sonarsource.com/sonarqube-server/latest/analyzing-source-code/scanners/sonarscanner-for-dotnet/)
- [.NET Test Coverage](https://docs.sonarsource.com/sonarqube-server/latest/analyzing-source-code/test-coverage/dotnet-test-coverage/)
