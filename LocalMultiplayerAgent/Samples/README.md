# MultiplayerSettings.json samples

This folder contains starter `MultiplayerSettings.json` files for the most common LocalMultiplayerAgent (LMA) configurations. To use one, copy its contents over `LocalMultiplayerAgent/MultiplayerSettings.json` (LMA always loads `MultiplayerSettings.json` from its working directory) and then edit the highlighted fields for your game server. If you're running the Linux Containers on Windows scenario, also pass the `-lcow` flag when launching LMA.

If you'd rather generate a settings file interactively, see the [MultiplayerSettings.json generator tool](../SettingsJsonGenerator/README.md).

For the canonical, end-to-end walkthrough of configuring and running LMA, see the [LocalMultiplayerAgent overview on Microsoft Learn](https://learn.microsoft.com/gaming/playfab/features/multiplayer/servers/localmultiplayeragent/local-multiplayer-agent-overview).

## Which sample should I use?

| Sample file | Host OS | Game server OS | Mode | Notes |
| --- | --- | --- | --- | --- |
| [`MultiplayerSettingsLinuxContainersOnWindowsSample.json`](MultiplayerSettingsLinuxContainersOnWindowsSample.json) | Windows | Linux | Container | Requires Docker Desktop with WSL 2. Launch LMA with the `-lcow` flag. See [`docs/lcow.md`](../../docs/lcow.md). |
| [`MultiplayerSettingsLinuxContainersOnLinuxSample.json`](MultiplayerSettingsLinuxContainersOnLinuxSample.json) | Linux (x64) | Linux | Container | Requires Docker. See [`docs/linux.md`](../../docs/linux.md). |
| [`MultiplayerSettingsLinuxContainersOnMacOSSample.json`](MultiplayerSettingsLinuxContainersOnMacOSSample.json) | macOS (Apple Silicon) | Linux | Container | Requires Docker Desktop for Mac. See [`docs/macos.md`](../../docs/macos.md). |
| [`MultiplayerSettingsLinuxProcessOnLinuxSample.json`](MultiplayerSettingsLinuxProcessOnLinuxSample.json) | Linux (x64) | Linux | Process | No Docker required; the agent runs your binary directly. |

For **Windows game servers running as a process on Windows** (the original LMA scenario), use the default [`../MultiplayerSettings.json`](../MultiplayerSettings.json) at the LMA root, which is already configured for that case.

> **Note on `GameServerEnvironment`:** there is no `GameServerEnvironment` field in `MultiplayerSettings.json`. LMA infers it: on macOS and Linux it is always `Linux`; on Windows it is `Windows` for process mode, remains `Windows` for Windows container runs, and switches to `Linux` only for the Linux Containers on Windows scenario when LMA is launched with the `-lcow` flag. See [`Program.cs`](../Program.cs).

## What you typically need to edit

For most users, only a handful of fields need to be changed from the sample defaults:

- **Container mode** — `ContainerStartParameters.ImageDetails` (`Registry`, `ImageName`, `ImageTag`, optional `Username`/`Password` for private registries) and `PortMappingsList[*].GamePort.Number` to match the port your game server listens on inside the container.
- **Process mode** — `AssetDetails[0].LocalFilePath` (path to your game server zip), `ProcessStartParameters.StartGameCommand` (executable inside the zip, optionally with arguments), and `PortMappingsList[*].GamePort.Number`.

Everything else can usually be left at the sample defaults.

---

## Field reference

Top-level fields in `MultiplayerSettings.json` are deserialized into [`MultiplayerSettings`](../Config/MultiplayerSettings.cs). Validation rules live in [`MultiplayerSettingsValidator`](../Config/MultiplayerSettingsValidator.cs). Field names are matched case-insensitively by the current loader, though using the documented casing is recommended for clarity.

### Top-level

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `RunContainer` | `bool` | yes | `true` runs the game server as a Docker container; `false` runs it as a process directly on the host. macOS only supports `true`. On Linux with Linux game servers, both modes are supported. |
| `OutputFolder` | `string` | no | Directory where LMA writes per-session log folders (`PlayFabVmAgentOutput/<timestamp>/...`). If empty or omitted, defaults to the directory of the LMA executable. Must exist if specified. |
| `NumHeartBeatsForActivateResponse` | `int` | yes | Number of heartbeats LMA receives before responding with `Active` (i.e., simulating a session allocation). Must be `> 0`. |
| `NumHeartBeatsForTerminateResponse` | `int` | yes | Number of heartbeats LMA receives before responding with `Terminate`. Must be `> 0` and strictly greater than `NumHeartBeatsForActivateResponse`. |
| `NumHeartBeatsForMaintenanceEventResponse` | `int` | no | Number of heartbeats before LMA simulates an Azure scheduled maintenance event. A value `< 1` (the default) disables maintenance event simulation. Must be `<= NumHeartBeatsForTerminateResponse`. |
| `MaintenanceEventType` | `string` | no | Type of simulated scheduled event (e.g., `Reboot`, `Redeploy`, `Freeze`, `Preempt`, `Terminate`). Only used when maintenance events are enabled. See [Azure scheduled events](https://learn.microsoft.com/azure/virtual-machines/windows/scheduled-events#event-properties). |
| `MaintenanceEventStatus` | `string` | no | Status of the simulated event (e.g., `Scheduled`, `Started`). |
| `MaintenanceEventSource` | `string` | no | Source of the simulated event (e.g., `Platform`, `User`). |
| `AgentListeningPort` | `int` | yes | TCP port on which LMA listens for GSDK heartbeats from the game server. This must be set explicitly in `MultiplayerSettings.json`; `56001` is the common sample value. If you change it, run the platform setup script (`Setup.ps1` / `setup_linux.sh` / `setup_macos.sh`) with the new port to open the firewall. |
| `TitleId` | `string` (hex) | yes (validation) | Hex string identifying your PlayFab title. May be left empty in the file; LMA fills it in with a random value via `SetDefaultsIfNotSpecified()`. |
| `BuildId` | `Guid` | yes (validation) | GUID identifying the build. Leave as `00000000-0000-0000-0000-000000000000` to have LMA generate one at startup. |
| `Region` | `string` | yes | Azure region name reported to the game server (e.g., `WestUs`). Any non-empty string is accepted locally. |
| `AssetDetails` | `AssetDetail[]` | conditional | Game server packages to extract before running. Required for process mode and for Windows containers. Optional for Linux containers (the image already contains the game). See [AssetDetails](#assetdetails) below. |
| `GameCertificateDetails` | `GameCertificateDetails[]` | no | Local `.pfx` certificates to install for the game server to consume. See [GameCertificateDetails](#gamecertificatedetails) below. |
| `PortMappingsList` | `PortMapping[][]` | yes | List of port-mapping lists, one inner list per session host. See [PortMappingsList](#portmappingslist) below. |
| `ContainerStartParameters` | `object` | required when `RunContainer = true` | Container image and start command. See [ContainerStartParameters](#containerstartparameters) below. |
| `ProcessStartParameters` | `object` | required when `RunContainer = false` | Process start command. See [ProcessStartParameters](#processstartparameters) below. |
| `SessionConfig` | `object` | no | Simulated allocation payload. If omitted, LMA creates a default `SessionConfig` with a generated `SessionId`. See [SessionConfig](#sessionconfig) below. |
| `ForcePullFromAcrOnLinuxContainersOnWindows` | `bool` | no | When `true`, LMA pulls the container image from the configured registry before starting on the Linux Containers on Windows scenario. Defaults to `false`. |
| `ForcePullContainerImageFromRegistry` | `bool` | no | When `true`, LMA pulls the container image from the registry before starting. Set to `true` when using a remote registry on macOS or Linux. Defaults to `false` (assumes the image is already available locally). |
| `DeploymentMetadata` | `Dictionary<string,string>` | no | Free-form key/value metadata exposed to the game server via GSDK. The samples use `Environment` and `FeaturesEnabled` as illustrative keys; any keys are accepted. |

### AssetDetails

Each entry in `AssetDetails` describes one game-server asset archive to extract into the container or process working directory. Use a local `.zip` file on Windows hosts; on Linux and macOS hosts, `.tar` and `.tar.gz` archives are also supported.

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `LocalFilePath` | `string` | yes | Absolute path to a local asset archive containing your game server. The file must exist when LMA starts. Supported formats are `.zip` on Windows hosts, and `.zip`, `.tar`, or `.tar.gz` on Linux and macOS hosts. |
| `MountPath` | `string` | yes for containers | Path **inside the container** where the asset is extracted (e.g., `C:\Assets` for Windows containers, `/data/Assets` for Linux containers). For Windows containers, the `StartGameCommand` must contain this path. Ignored / set to `null` in process mode. |

Notes:

- For Linux containers, `AssetDetails` is **optional** — the entire game can be packaged into the container image instead.
- For process mode and Windows containers, at least one `AssetDetails` entry is required.

### PortMappingsList

`PortMappingsList` is a list of lists. Each inner list represents the ports for one session host; LMA only runs a single session host locally, so you'll typically have exactly one inner list.

Each port-mapping object:

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `NodePort` | `int` | yes when `RunContainer = true` | The host (VM) port that gets mapped to the container's `GamePort.Number`. Clients connect to this port locally. |
| `PublicPort` | `int` | no | The externally reachable port. If omitted or `0`, LMA sets it equal to `NodePort` (LMA always runs locally so the two are the same). |
| `GamePort.Name` | `string` | yes | Friendly name for the port, exposed to the game server via GSDK (e.g., `gameport`). |
| `GamePort.Number` | `int` | yes | The port the game server listens on (inside the container, for container mode; on the host, for process mode). |
| `GamePort.Protocol` | `string` | yes | `TCP` or `UDP`. |

### ContainerStartParameters

Used only when `RunContainer = true`.

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `StartGameCommand` | `string` | conditional | The command to run inside the container. Required for Windows containers (and must reference the asset `MountPath`). Optional for Linux containers if the image's `CMD`/`ENTRYPOINT` already starts the game. |
| `ImageDetails.Registry` | `string` | yes | FQDN of the container registry (e.g., `mcr.microsoft.com`, `myregistry.azurecr.io`, `mydockerregistry.io`). |
| `ImageDetails.ImageName` | `string` | yes | Image repository name (e.g., `playfab/multiplayer`, `mygame`). |
| `ImageDetails.ImageTag` | `string` | yes (unless using digest) | Image tag (e.g., `wsc-10.0.20348.3207`, `0.1`). |
| `ImageDetails.ImageDigest` | `string` | no | Image digest. Takes precedence over `ImageTag` when set. |
| `ImageDetails.Username` | `string` | no | Registry username (for private registries). Leave empty for public registries. |
| `ImageDetails.Password` | `string` | no | Registry password / token. Leave empty for public registries. **Do not commit secrets** — these values are redacted from logs by LMA. |
| `ResourceLimits.Cpus` | `double` | no | Maximum CPUs the container may use (Docker `--cpus`). `0` (default) means no limit. |
| `ResourceLimits.MemoryGib` | `double` | no | Maximum memory the container may use, in GiB (Docker `--memory`). `0` (default) means no limit. |

### ProcessStartParameters

Used only when `RunContainer = false`.

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `StartGameCommand` | `string` | yes | The command LMA runs after extracting the asset zip. Resolved relative to the extracted asset directory; arguments may be appended (e.g., `MyServer.exe -port 7777` or `./MyServer --port 7777`). |

### SessionConfig

Values used to simulate the session allocation payload that the production MPS service would normally send.

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `SessionId` | `Guid` | yes | GUID identifying the simulated session. |
| `SessionCookie` | `string` | no | Opaque string passed to the game server in the allocation. LMA prints a warning if not specified. |
| `InitialPlayers` | `string[]` | no | Initial player IDs included in the simulated allocation. |
| `Metadata` | `Dictionary<string,string>` | no | Free-form session metadata exposed to the game server. |

### GameCertificateDetails

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `Name` | `string` | yes | Friendly name the game server uses to look up the certificate. Must be unique within the array. |
| `Path` | `string` | yes | Local filesystem path to a `.pfx` certificate file. Must exist and end with `.pfx`. Each path must be unique within the array. |

---

## See also

- [LocalMultiplayerAgent overview on Microsoft Learn](https://learn.microsoft.com/gaming/playfab/features/multiplayer/servers/localmultiplayeragent/local-multiplayer-agent-overview)
- [`SettingsJsonGenerator/README.md`](../SettingsJsonGenerator/README.md) — interactive generator for `MultiplayerSettings.json`
- [`BuildTool/readme.md`](../BuildTool/readme.md) — companion tool for creating MPS builds via LMA
- Platform-specific guides: [`docs/lcow.md`](../../docs/lcow.md), [`docs/linux.md`](../../docs/linux.md), [`docs/macos.md`](../../docs/macos.md)
