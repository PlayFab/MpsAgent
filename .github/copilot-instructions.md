# Copilot Instructions for MpsAgent

## Build and Test

```bash
# Build the full solution
dotnet build --configuration Release

# Run all tests
dotnet test

# Run tests for a single project
dotnet test VmAgent.Core.UnitTests
dotnet test AgentInterfaces.Tests
dotnet test LocalMultiplayerAgent.UnitTest

# Run a single test by fully qualified name
dotnet test --filter "FullyQualifiedName~VmAgent.Core.UnitTests.LogSanitizerTests.Sanitize_ShouldRedactSensitiveData"

# Publish LocalMultiplayerAgent as single-file executable
dotnet publish LocalMultiplayerAgent --runtime win-x64 -c Release -o LocalMultiplayerAgentPublishFolder -p:PublishSingleFile=true --self-contained true
```

## Architecture

This is a .NET 8 solution for Azure PlayFab Multiplayer Servers (MPS). It has three layers:

- **AgentInterfaces** — Shared data models and enums (e.g., `SessionHostStatus`, `SessionHostHeartbeatInfo`, `VmState`). Published as NuGet package `PlayFab.MultiplayerServers.AgentInterfaces`. Used by both LocalMultiplayerAgent and the production MPS service.
- **VmAgent.Core** — Core VM agent logic: session host lifecycle management, Docker container orchestration, process running, asset extraction, and log sanitization. Published as NuGet package `PlayFab.MultiplayerServers.VmAgent.Core`. Also used by the production service.
- **LocalMultiplayerAgent** — An ASP.NET Core web app that mimics the production MPS agent for local debugging. It hosts a Kestrel HTTP server that game servers send heartbeats to. Configured via `MultiplayerSettings.json`.

### Session Host Lifecycle

Game servers communicate with the agent via heartbeats. The status flow is:
`PendingHeartbeat → Initializing → StandingBy → Active → Terminating → Terminated`

The agent responds to heartbeats with operations: `Continue`, `Active` (allocate session), or `Terminate`.

### Key Abstractions

- **`ISessionHostRunner`** — Interface for running game servers. Two implementations: `DockerContainerEngine` (containers) and `ProcessRunner` (bare processes).
- **`SessionHostRunnerFactory`** — Creates the appropriate runner based on `SessionHostType` (Container or Process).
- **`ISessionHostConfiguration`** / `SessionHostConfigurationBase` — Configures GSDK settings, port mappings, and paths. `SessionHostContainerConfiguration` and `SessionHostProcessConfiguration` differ in how ports and paths are resolved (container-scoped vs VM-scoped).
- **`ISystemOperations`** — Abstraction over file system, process execution, and OS operations. Enables testability via mocking.
- **`ISessionHostManager`** — Manages session host state, heartbeat processing, and allocation. `NoOpSessionHostManager` is used locally.

### Configuration

- `MultiplayerSettings.json` — Main config file for LocalMultiplayerAgent. Controls container vs process mode (`RunContainer`), port mappings, asset paths, heartbeat thresholds, and session config.
- `Globals` — Static class holding runtime state (settings, VM config, logger, environment).

## Conventions

- **Namespaces**: Root namespace is `Microsoft.Azure.Gaming`. Sub-namespaces: `AgentInterfaces`, `VmAgent.Core`, `VmAgent.Core.Interfaces`, `VmAgent.Model`, `LocalMultiplayerAgent`, `LocalMultiplayerAgent.Config`.
- **JSON serialization**: Uses Newtonsoft.Json throughout. All enums in AgentInterfaces must have `[JsonConverter(typeof(StringEnumConverter))]` — enforced by a test.
- **Testing**: MSTest framework with `[TestClass]`/`[TestMethod]` attributes. Tests use `[TestCategory("BVT")]`. VmAgent.Core tests use Moq for mocking and FluentAssertions. JSON fixture files are used for deserialization tests.
- **Logging**: Uses `MultiLogger` (wraps `ILogger`) and `LogSanitizer` to redact secrets (keys, tokens, passwords, SAS signatures) before logging.
- **Copyright header**: All source files start with `// Copyright (c) Microsoft Corporation.` and `// Licensed under the MIT License.`
- **Do not modify** the YAML pipeline files (`AgentInterfaces.yml`, `LocalMultiplayerAgent.yml`, `VmAgent.Core.yml`) — they are used by internal Azure DevOps pipelines.
