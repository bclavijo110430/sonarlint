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

El proyecto incluye controladores y servicios con issues tipicos y vulnerabilidades de seguridad que SonarQube/SonarLint detectaran:

### Code smells y bugs

| Ubicacion | Issue esperado | Regla aproximada |
|---|---|---|
| `GET /issues/unused` | Variable asignada pero no usada | S1481 |
| `GET /issues/empty-catch` | Bloque `catch` vacio | S108 |
| `GET /issues/null-reference` | Desreferencia posiblemente nula | S2259 |
| `GET /issues/duplicate-a` / `duplicate-b` | Codigo duplicado | S1192 / S4144 |
| `GET /issues/search` | Posible inyeccion SQL | S3649 |
| `GET /newcode/baseline` | Comentario TODO sin resolver | S1135 |
| `POST /newcode/calculate` | Numero magico | S109 |
| `GET /newcode/grade` | Complejidad cognitiva | S3776 |
| `LegacyService.ProcessOrder` | Demasiados parametros | S107 |
| `UserInput.RawValue` | Campo publico | S1104 |

### Vulnerabilidades de seguridad

| Ubicacion | Issue esperado | Regla aproximada |
|---|---|---|
| `GET /security/login` | Contrasena hardcoded | S2068 |
| `GET /security/read-file` | Path traversal | S2083 |
| `GET /security/run` | Command injection | S2076 |
| `GET /security/search-xml` | XPath injection | S2091 |
| `GET /security/echo` | XSS (Cross-Site Scripting) | S5131 |
| `GET /security/redirect` | Open redirect | S5144 |
| `GET /security/fetch` | SSRF | S5334 |
| `GET /security/hash` | Criptografia debil (MD5) | S4426 |
| `GET /security/cookie` | Cookie insegura | S2092 / S4787 |
| `SecurityService.ApiKey` | API key hardcoded | S2068 |
| `SecurityService.FindUsers` | SQL injection | S2077 |
| `SecurityService.HashToken` | Hash debil (SHA1) | S4790 |

---

## Scripts disponibles

| Script | Descripcion |
|---|---|
| `infra/init-sonarqube.ps1` | Configura `vm.max_map_count` en la maquina Podman |
| `scripts/Start-SonarQube.ps1` | Levanta SonarQube y PostgreSQL |
| `scripts/Stop-SonarQube.ps1` | Detiene los contenedores. Acepta `-ComposeProvider` |
| `scripts/Initialize-Project.ps1` | Prepara tools .NET y proyecto en SonarQube |
| `scripts/Start-Analysis.ps1` | Ejecuta SonarScanner con cobertura de tests |
| `scripts/Add-NewCodeSample.ps1` | Agrega codigo nuevo para probar "New Code" |

---

## Probar la funcionalidad "New Code"

SonarQube Community Build y SonarLint permiten enfocarse en **codigo nuevo**: issues introducidos despues de una linea base.

### Flujo recomendado

1. **Analisis inicial (baseline)**:
   ```powershell
   .\scripts\Start-Analysis.ps1 -SonarToken <tu-token>
   ```

2. **Agregar codigo nuevo**:
   ```powershell
   .\scripts\Add-NewCodeSample.ps1
   ```
   Este script agrega metodos adicionales a `NewCodeController.cs` con nuevos issues.

3. **Segundo analisis**:
   ```powershell
   .\scripts\Start-Analysis.ps1 -SonarToken <tu-token>
   ```

4. **Revisar resultados**:
   - Abre el proyecto en SonarQube.
   - Ve a la pestana **"New Code"** o **"Overall Code"**.
   - Los issues agregados en el paso 2 apareceran como codigo nuevo.

> **Nota**: En SonarQube Community Build, la definicion de "New Code" por defecto es "Previous version" o "Previous analysis". Si no ves los cambios como "new code", verifica la configuracion en **Project Settings > New Code**.

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

### SonarLint no detecta findings

Si la vista **SonarQube Findings** muestra "No SonarQube issues to display", revisa lo siguiente:

#### 1. Extensiones requeridas

Para analizar C# en VS Code, SonarQube for IDE requiere la extension **C# Dev Kit** (o la extension C# basica). Asegurate de tenerlas instaladas:

- `ms-dotnettools.csdevkit`
- `ms-dotnettools.vscode-dotnet-runtime`
- `SonarSource.sonarlint-vscode`

#### 2. Solucion cargada

Abre la paleta de comandos (`Ctrl+Shift+P`) y ejecuta:

```
.NET: Open Solution
```

Selecciona `SonarLintDemo.sln`. El proyecto debe compilar sin errores antes de que SonarLint pueda analizarlo.

#### 3. Connected Mode configurado

El binding del proyecto esta en `.vscode/settings.json`, pero la **conexion al servidor** debe estar en tus **User Settings** de VS Code. Presiona `Ctrl+Shift+P` y ejecuta:

```
Preferences: Open User Settings (JSON)
```

Agrega lo siguiente (reemplaza `<tu-token>`):

```json
{
  "sonarlint.connectedMode.connections.sonarqube": [
    {
      "connectionId": "local-sonarqube",
      "serverUrl": "http://localhost:9000",
      "token": "<tu-token>"
    }
  ]
}
```

> **Importante**: El `connectionId` debe coincidir exactamente con el de `.vscode/settings.json` (`local-sonarqube`).

#### 4. Verificar conexion

1. Abre la vista **SONARQUBE SETUP > CONNECTED MODE** en la barra lateral.
2. Deberias ver la conexion `local-sonarqube`.
3. Si te pide "Bind project", acepta y selecciona `SonarLintDemo`.

#### 5. Ver logs detallados

Habilita logs verbose para diagnosticar:

1. `Ctrl+Shift+P` > `Preferences: Open User Settings (JSON)`.
2. Agrega:
   ```json
   "sonarlint.output.showVerboseLogs": true,
   "sonarlint.output.showAnalyzerLogs": true
   ```
3. Reinicia VS Code.
4. Abre **View > Output > SonarQube for IDE**.
5. Abre un archivo `.cs` y revisa si hay errores de analisis.

#### 6. Reiniciar extensiones

Si despues de configurar todo sigue sin funcionar:

1. Cierra todos los archivos `.cs`.
2. Abre la paleta de comandos y ejecuta:
   ```
   Developer: Reload Window
   ```
3. Abre un archivo como `IssuesController.cs` y espera 30-60 segundos a que SonarLint descargue el analizador de C#.

#### 7. Limitaciones de C# en SonarLint

- Algunas reglas de seguridad avanzadas (inyeccion SQL, XSS, etc.) requieren **Connected Mode** y que el proyecto haya sido analizado primero por SonarQube Server.
- SonarLint para VS Code analiza archivos abiertos. Si no tienes ningun archivo `.cs` abierto, no veras findings.
- El analizador de C# se descarga en segundo plano la primera vez. Requiere conexion a Internet.

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
