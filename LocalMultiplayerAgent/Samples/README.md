# MultiplayerSettings.json samples

Starter `MultiplayerSettings.json` files for the most common LocalMultiplayerAgent (LMA) configurations.

## Prerequisites

Have these in place before your first run:

- **The LMA binary.** Either grab a prebuilt release or build it from source from the repo root:

  ```bash
  # Windows
  dotnet publish LocalMultiplayerAgent --runtime win-x64    -c Release -o LocalMultiplayerAgentPublishFolder -p:PublishSingleFile=true --self-contained true
  # Linux
  dotnet publish LocalMultiplayerAgent --runtime linux-x64  -c Release -o LocalMultiplayerAgentPublishFolder -p:PublishSingleFile=true --self-contained true
  # macOS (Apple Silicon)
  dotnet publish LocalMultiplayerAgent --runtime osx-arm64  -c Release -o LocalMultiplayerAgentPublishFolder -p:PublishSingleFile=true --self-contained true
  ```

  Building from source requires the [.NET 8 SDK](https://dotnet.microsoft.com/download).

- **Docker**, if you are running in container mode:
  - **Windows containers (Windows host):** Docker Desktop, switched to Windows containers.
  - **Linux Containers on Windows (LCOW) (Windows host):** Docker Desktop with the WSL 2 backend. See [`docs/lcow.md`](../../docs/lcow.md).
  - **Linux containers (Linux host):** Docker Engine.
  - **Linux containers (macOS host, Apple Silicon):** Docker Desktop for Mac.

- **Run the platform setup script** from the `LocalMultiplayerAgent/` directory once, to open the firewall (and configure the LCOW Docker network where applicable):
  - Windows: `Setup.ps1` opens the firewall for `AgentListeningPort` (default `56001`). For LCOW you additionally need `SetupLinuxContainersOnWindows.ps1`, which configures the Docker network LMA expects.
  - Linux: `setup_linux.sh`
  - macOS: `setup_macos.sh`

## Quickstart

1. **Pick a sample** that matches your host OS and game-server OS — see [Choosing a sample](#choosing-a-sample) below.
2. **Copy it over** `LocalMultiplayerAgent/MultiplayerSettings.json` (LMA always loads that file from its working directory). The default `MultiplayerSettings.json` is itself the Windows/Windows sample, so for that scenario you can just edit it in place.
3. **Edit a few fields** for your game — usually just `ImageDetails` / `AssetDetails`, `StartGameCommand`, and `PortMappingsList`. See [What you typically need to edit](#what-you-typically-need-to-edit).
4. **Run LMA** from its working directory (the same directory that holds `MultiplayerSettings.json`):
   - Windows: `LocalMultiplayerAgent.exe` (add the `-lcow` flag for the Linux Containers on Windows scenario)
   - Linux / macOS: `./LocalMultiplayerAgent`

> Skim [Common pitfalls](#common-pitfalls) before your first run — several of them are first-run validation failures.

Prefer an interactive setup? Use the [MultiplayerSettings.json generator](../SettingsJsonGenerator/README.md). For the canonical, end-to-end walkthrough of configuring and running LMA, see the [LocalMultiplayerAgent overview on Microsoft Learn](https://learn.microsoft.com/gaming/playfab/features/multiplayer/servers/localmultiplayeragent/local-multiplayer-agent-overview).

## Contents

- [Prerequisites](#prerequisites)
- [Quickstart](#quickstart)
- [Choosing a sample](#choosing-a-sample)
- [What you typically need to edit](#what-you-typically-need-to-edit)
  - [Container mode (image and ports)](#container-mode-image-and-ports)
  - [Container mode with separate game assets](#container-mode-with-separate-game-assets-windows-containers-or-linux-containers-where-the-image-doesnt-include-the-game)
  - [Process mode](#process-mode)
- [Common pitfalls](#common-pitfalls)
- [Field reference](#field-reference)
- [See also](#see-also)

## Choosing a sample

| Sample file | Host OS | Game server OS | Mode | Notes |
| --- | --- | --- | --- | --- |
| [`../MultiplayerSettings.json`](../MultiplayerSettings.json) (default) | Windows | Windows | Process **or** Container | Ships in Process mode (`RunContainer: false`) — the original LMA scenario. To switch to Windows containers, set `RunContainer: true`; the bundled `ContainerStartParameters` already targets a Windows Server Core image, just ensure `StartGameCommand` references the asset `MountPath` (e.g., `C:\Assets\MyServer.exe`). |
| [`MultiplayerSettingsLinuxContainersOnWindowsSample.json`](MultiplayerSettingsLinuxContainersOnWindowsSample.json) | Windows | Linux | Container (LCOW) | Requires Docker Desktop with WSL 2. Launch LMA with the `-lcow` flag. See [`docs/lcow.md`](../../docs/lcow.md). |
| [`MultiplayerSettingsLinuxContainersOnLinuxSample.json`](MultiplayerSettingsLinuxContainersOnLinuxSample.json) | Linux (x64) | Linux | Container | Requires Docker. See [`docs/linux.md`](../../docs/linux.md). |
| [`MultiplayerSettingsLinuxContainersOnMacOSSample.json`](MultiplayerSettingsLinuxContainersOnMacOSSample.json) | macOS (Apple Silicon) | Linux | Container | Requires Docker Desktop for Mac. See [`docs/macos.md`](../../docs/macos.md). |
| [`MultiplayerSettingsLinuxProcessOnLinuxSample.json`](MultiplayerSettingsLinuxProcessOnLinuxSample.json) | Linux (x64) | Linux | Process | No Docker required; the agent runs your binary directly. |

> **Note on `GameServerEnvironment`:** there is no `GameServerEnvironment` field in `MultiplayerSettings.json`. LMA infers it: on macOS and Linux it is always `Linux`; on Windows it is `Windows` for process mode, remains `Windows` for Windows container runs, and switches to `Linux` only for the Linux Containers on Windows (LCOW) scenario when LMA is launched with the `-lcow` flag. See [`Program.cs`](../Program.cs).

## What you typically need to edit

For most users, only a handful of fields differ from the sample defaults. The blocks below are **patches** — the keys you usually need to overwrite inside the top-level object of your chosen sample file. Keep the surrounding `{ ... }` from the sample, and strip the `// ...` annotations before saving (LMA's JSON parser tolerates `//` comments, but it's clearer to leave them out of your real config).

### Container mode (image and ports)

Point LMA at your image and tell it which port the game server listens on inside the container. **By itself, this snippet is sufficient for Linux containers where the game is baked into the image** — the most common starting point on Linux and macOS hosts. For Windows containers, or for Linux containers where the image doesn't already contain the game, also apply the patch in [Container mode with separate game assets](#container-mode-with-separate-game-assets-windows-containers-or-linux-containers-where-the-image-doesnt-include-the-game) below.

```jsonc
"RunContainer": true,
"ContainerStartParameters": {
  "ImageDetails": {
    "Registry": "myregistry.azurecr.io",  // your registry; omit for Docker Hub
    "ImageName": "mygame",                // your image
    "ImageTag": "0.1",                    // optional; defaults to "latest" if omitted
    "Username": "",                       // private registries only
    "Password": ""
  }
},
"PortMappingsList": [
  [
    { "NodePort": 56100, "GamePort": { "Name": "gameport", "Number": 7777, "Protocol": "TCP" } }
  ]
]
```

**Image-pull behavior at a glance:**

| Scenario | Pull behavior |
| --- | --- |
| Windows containers on Windows | Always pulls from the registry, regardless of either `ForcePull*` flag. |
| LCOW (Linux Containers on Windows) | Pulls only when `ForcePullFromAcrOnLinuxContainersOnWindows` is `true`. |
| Linux / macOS containers | Pulls only when `ForcePullContainerImageFromRegistry` is `true`. Leave it `false` (the default) if you built the image locally with `docker build` and want LMA to use the local image. |

**Private registries.** If your registry needs authentication, supply `Username`/`Password` directly in `ImageDetails`. LMA passes them to Docker as in-process credentials — there is no separate `docker login` step required, and no Azure Container Registry-specific auth mode (token / managed identity / service principal) is wired in for LMA. Treat the values as secrets and don't commit them.

### Container mode with separate game assets (Windows containers, or Linux containers where the image doesn't include the game)

**Windows containers cannot bake the game into the image** in the LMA workflow — you must always supply `AssetDetails`, and the `StartGameCommand` must reference one of the configured `MountPath` values (the validator enforces this). Linux containers may either bake the game into the image (omit `AssetDetails`) or mount it from a local archive using the patch below.

In addition to the snippet above, define `AssetDetails` and point `StartGameCommand` at the mounted path. The example uses a Windows container; for a Linux container, use Linux-style paths (e.g., `LocalFilePath: "/path/to/game.tar.gz"`, `MountPath: "/data/Assets"`, `StartGameCommand: "/data/Assets/MyServer"`).

```jsonc
"RunContainer": true,
"AssetDetails": [
  { "LocalFilePath": "C:\\path\\to\\game.zip", "MountPath": "C:\\Assets" }
],
"ContainerStartParameters": {
  "StartGameCommand": "C:\\Assets\\MyServer.exe",  // must contain the MountPath above
  "ImageDetails": {
    "Registry": "mcr.microsoft.com",      // example: a Windows Server Core base image
    "ImageName": "windows/servercore",
    "ImageTag": "ltsc2022"
  }
}
```

Substitute the `ImageDetails` values that match your own image (the same shape as in the [previous snippet](#container-mode-image-and-ports)).

### Process mode

Process mode runs your game-server binary directly on the host (Windows or Linux — macOS is container-only). The example below is Windows-primary; substitute Linux paths and command syntax where indicated:

```jsonc
"AssetDetails": [
  { "LocalFilePath": "C:\\path\\to\\game.zip" }   // on Linux: "/path/to/game.tar.gz" or "/path/to/game.zip"; MountPath is ignored
],
"ProcessStartParameters": {
  "StartGameCommand": "MyServer.exe -port 7777"   // path is relative to the extracted asset directory; on Linux: "./MyServer --port 7777"
},
"PortMappingsList": [
  [
    { "NodePort": 56100, "GamePort": { "Name": "gameport", "Number": 7777, "Protocol": "TCP" } }
  ]
]
```

Everything else can usually be left at the sample defaults. Skim [Common pitfalls](#common-pitfalls) below before your first run.

---

## Common pitfalls

A few first-run failure modes worth knowing about:

- **`OutputFolder` must exist if you set it.** The `MultiplayerSettingsLinuxContainersOnWindowsSample.json` sample sets `OutputFolder` to `C:\output\UnityServerLinux`, which doesn't exist on a fresh install. Either create that directory before running, or clear the field — LMA will then default to the agent's executable directory.
- **A non-default `AgentListeningPort` requires a firewall update.** If you change `AgentListeningPort` away from `56001`, re-run the platform setup script (`Setup.ps1` / `setup_linux.sh` / `setup_macos.sh`) with the new port to open it. (For LCOW, `SetupLinuxContainersOnWindows.ps1` configures the Docker network and is separate from the firewall script — both may apply.)
- **Windows containers always pull from the registry.** When LMA's `GameServerEnvironment` resolves to `Windows` (Windows containers on Windows), the image is pulled before each run regardless of `ForcePullContainerImageFromRegistry`. The flag only takes effect on macOS, Linux, and the LCOW (Linux Containers on Windows) scenarios.
- **Local-image workflow on Linux / macOS:** if you want LMA to use an image you just built with `docker build` (instead of pulling), keep `ForcePullContainerImageFromRegistry: false`. The bundled `MultiplayerSettingsLinuxContainersOnMacOSSample.json` ships with this set to `true`, which will fail unless the image has been pushed to a remote registry first.
- **Windows-container `StartGameCommand` must reference an asset `MountPath`.** The validator rejects bare commands like `MyServer.exe`; use the full mounted path (e.g., `C:\Assets\MyServer.exe` matching `AssetDetails[0].MountPath`). Windows containers also cannot bake the game into the image — `AssetDetails` is always required.
- **macOS does not support process mode.** `RunContainer` must be `true` on macOS hosts.
- **`<your_game_server_exe>` placeholder is rejected by name.** The validator rejects any `StartGameCommand` whose value *contains the substring* `<your_game_server_exe` — that catches both the short `<your_game_server_exe>` placeholder (default `MultiplayerSettings.json`) and the longer `<your_game_server_executable_with_arguments>` placeholder used in `MultiplayerSettingsLinuxProcessOnLinuxSample.json`. Replace with the real executable path before running.
- **`<path_to_game_server_package>` produces a generic "file not found" error.** Unlike the placeholder above, this isn't detected as a placeholder; it fails the regular `LocalFilePath` existence check (`Asset <path_to_game_server_package> was not found. Please specify path to a local zip file.`). Replace with a real local archive path.
- **`//` comments in this README are illustrative.** The default JSON loader (Newtonsoft.Json) does tolerate `//` and `/* */` comments, but strip them out of your real `MultiplayerSettings.json` so casual readers (and stricter JSON tools) aren't surprised.

---

## Field reference

Top-level fields in `MultiplayerSettings.json` are deserialized into [`MultiplayerSettings`](../Config/MultiplayerSettings.cs). Validation rules live in [`MultiplayerSettingsValidator`](../Config/MultiplayerSettingsValidator.cs). Field names are matched case-insensitively by the current loader, though using the documented casing is recommended for clarity.

### Top-level

The top-level fields are split into five thematic groups for readability. Within each group, fields are arranged for pedagogical flow rather than strict source-declaration order — refer to [`MultiplayerSettings.cs`](../Config/MultiplayerSettings.cs) for the canonical declaration order if needed.

**Runtime mode**

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `RunContainer` | `bool` | yes (in practice) | `true` runs the game server as a Docker container; `false` runs it as a process directly on the host. C# default is `false` and the validator does not require an explicit value, but you should always set this consciously: macOS only supports `true`; on Linux with Linux game servers, both modes are supported. |
| `OutputFolder` | `string` | no | Directory where LMA writes per-session log folders (`PlayFabVmAgentOutput/<timestamp>/...`). If empty or omitted, defaults to the directory of the LMA executable. Must exist if specified. |
| `AgentListeningPort` | `int` | yes | TCP port on which LMA listens for GSDK heartbeats from the game server. This must be set explicitly in `MultiplayerSettings.json`; `56001` is the common sample value. If you change it, run the platform setup script (`Setup.ps1` / `setup_linux.sh` / `setup_macos.sh`) with the new port to open the firewall. |

**Heartbeat & maintenance-event simulation**

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `NumHeartBeatsForActivateResponse` | `int` | yes | Number of heartbeats LMA receives before responding with `Active` (i.e., simulating a session allocation). Must be `> 0`. |
| `NumHeartBeatsForTerminateResponse` | `int` | yes | Number of heartbeats LMA receives before responding with `Terminate`. Must be `> 0` and strictly greater than `NumHeartBeatsForActivateResponse`. |
| `NumHeartBeatsForMaintenanceEventResponse` | `int` | no | Number of heartbeats before LMA simulates an Azure scheduled maintenance event. A value `< 1` (the default) disables maintenance event simulation. Must be `<= NumHeartBeatsForTerminateResponse`. |
| `MaintenanceEventType` | `string` | no | Type of simulated scheduled event (e.g., `Reboot`, `Redeploy`, `Freeze`, `Preempt`, `Terminate`). Only used when maintenance events are enabled. See [Azure scheduled events](https://learn.microsoft.com/azure/virtual-machines/windows/scheduled-events#event-properties). |
| `MaintenanceEventStatus` | `string` | no | Status of the simulated event (e.g., `Scheduled`, `Started`). |
| `MaintenanceEventSource` | `string` | no | Source of the simulated event (e.g., `Platform`, `User`). |

**Identity**

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `TitleId` | `string` (hex) | no (auto-generated) | Hex string identifying your PlayFab title. Leave empty in the file to have LMA fill it in with a random value via `SetDefaultsIfNotSpecified()`; the validator then requires the post-default value to be a valid hex string. |
| `BuildId` | `Guid` | no (auto-generated) | GUID identifying the build. Leave as `00000000-0000-0000-0000-000000000000` to have LMA generate one at startup. |
| `Region` | `string` | yes | Azure region name reported to the game server (e.g., `WestUs`). Any non-empty, non-whitespace string is accepted locally. |

**Game-server configuration**

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `AssetDetails` | `AssetDetail[]` | conditional | Game server packages to extract before running. Required for process mode and for Windows containers. Optional for Linux containers when the game is baked into the image. See [AssetDetails](#assetdetails) below. |
| `GameCertificateDetails` | `GameCertificateDetails[]` | no | Local `.pfx` certificates to install for the game server to consume. See [GameCertificateDetails](#gamecertificatedetails) below. |
| `PortMappingsList` | `List<List<PortMapping>>` | yes | List of port-mapping lists, one inner list per session host. LMA always runs a single session host locally and only uses the **first** inner list (`PortMappingsList[0]`); additional inner lists are accepted by the loader but are not exercised at runtime (the validator only walks them in container mode, to verify `NodePort != 0`). See [PortMappingsList](#portmappingslist) below. |
| `ContainerStartParameters` | `object` | required when `RunContainer = true` | Container image and start command. See [ContainerStartParameters](#containerstartparameters) below. |
| `ProcessStartParameters` | `object` | required when `RunContainer = false` | Process start command. See [ProcessStartParameters](#processstartparameters) below. |
| `SessionConfig` | `object` | no | Simulated allocation payload. If omitted, LMA creates a default `SessionConfig` with a generated `SessionId`. See [SessionConfig](#sessionconfig) below. |
| `DeploymentMetadata` | `IDictionary<string,string>` | no | Free-form key/value metadata exposed to the game server via GSDK. The samples use `Environment` and `FeaturesEnabled` as illustrative keys; LMA accepts any keys with no count limit. **Note:** the production MPS service caps this at 30 entries (see [`docs/linux.md`](../../docs/linux.md)). |

**Image-pull behavior**

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `ForcePullFromAcrOnLinuxContainersOnWindows` | `bool` | no | When `true`, LMA pulls the container image from the configured registry before starting on the Linux Containers on Windows scenario. Defaults to `false`. |
| `ForcePullContainerImageFromRegistry` | `bool` | no | When `true`, LMA pulls the container image from the registry before starting. Set to `true` when using a remote registry on macOS or Linux. Defaults to `false` (skip the pull, assuming the image is locally available, e.g., from `docker build`). **Note:** for Windows containers (when `GameServerEnvironment` resolves to `Windows`), LMA always pulls regardless of this flag. |

### AssetDetails

Each entry in `AssetDetails` describes one game-server asset archive to extract into the container or process working directory. Use a local `.zip` file on Windows hosts; on Linux and macOS hosts, `.tar` and `.tar.gz` archives are also supported.

LMA only consumes `LocalFilePath` and `MountPath`. The production MPS service additionally supports `DownloadUris` and `SasTokens` for downloading remote assets, but those fields are **not** read by LMA — package your asset locally before running.

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `LocalFilePath` | `string` | yes | Absolute path to a local asset archive containing your game server. The file must exist when LMA starts. Supported formats are `.zip` on Windows hosts, and `.zip`, `.tar`, or `.tar.gz` on Linux and macOS hosts. |
| `MountPath` | `string` | yes for containers | Path **inside the container** where the asset is extracted (e.g., `C:\Assets` for Windows containers, `/data/Assets` for Linux containers). For Windows containers, the `StartGameCommand` must contain this path. Ignored / set to `null` in process mode. |

Notes:

- For Linux containers, `AssetDetails` is **optional** — the entire game can be packaged into the container image instead.
- For process mode and Windows containers, at least one `AssetDetails` entry is required.
- Each `MountPath` should be unique across `AssetDetails` entries. LMA does not validate this, but Docker creates one bind mount per entry; reusing the same destination can cause container creation to fail or hide files.

### PortMappingsList

`PortMappingsList` is a list of lists. Each inner list represents the ports for one session host; LMA only runs a single session host locally and reads exclusively from `PortMappingsList[0]`, so you'll typically have exactly one inner list. Additional inner lists are accepted by the loader but never started (and only walked by the validator in container mode, to check `NodePort != 0`).

Each port-mapping object:

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `NodePort` | `int` | yes when `RunContainer = true` | The host (VM) port that gets mapped to the container's `GamePort.Number`. Clients connect to this port locally. |
| `PublicPort` | `int` | no | The externally reachable port. If omitted or `0`, LMA sets it equal to `NodePort` (LMA always runs locally so the two are the same). |
| `GamePort.Name` | `string` | yes | Friendly name for the port, exposed to the game server via GSDK (e.g., `gameport`). |
| `GamePort.Number` | `int` | yes for containers; effectively unvalidated for process mode | The port the game server listens on (inside the container, for container mode; on the host, for process mode). The validator only inspects `PortMappingsList` when `RunContainer = true`, so process-mode runs accept any value — but you should still set it correctly so it propagates to the game server via GSDK. |
| `GamePort.Protocol` | `string` | yes | `TCP` or `UDP`. |

### ContainerStartParameters

Used only when `RunContainer = true`.

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `StartGameCommand` | `string` | conditional | The command to run inside the container. Required for Windows containers (and must reference the asset `MountPath`). Optional for Linux containers if the image's `CMD`/`ENTRYPOINT` already starts the game. |
| `ImageDetails.Registry` | `string` | conditional | FQDN of the container registry (e.g., `mcr.microsoft.com`, `myregistry.azurecr.io`). Optional when the image is on Docker Hub or already cached locally. Effectively required when `ForcePullContainerImageFromRegistry` (or, on LCOW, `ForcePullFromAcrOnLinuxContainersOnWindows`) is `true` and the image isn't on Docker Hub. |
| `ImageDetails.ImageName` | `string` | yes | Image repository name (e.g., `playfab/multiplayer`, `mygame`). |
| `ImageDetails.ImageTag` | `string` | no | Optional image tag (e.g., `wsc-10.0.20348.3207`, `0.1`). If omitted, LMA defaults to `latest`. |
| `ImageDetails.ImageDigest` | `string` | no | Image digest. Currently not used by LMA's `DockerContainerEngine`; specify `ImageTag` for image selection. |
| `ImageDetails.Username` | `string` | no | Registry username (for private registries). Leave empty for public registries. |
| `ImageDetails.Password` | `string` | no | Registry password / token. Leave empty for public registries. **Do not commit secrets** — these values are never logged in LMA's hot paths (and `ContainerImageDetails.ToRedacted()` strips them when objects are surfaced for diagnostics). |
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
| `SessionId` | `Guid` | conditional | GUID identifying the simulated session. **Auto-generated only when the entire `SessionConfig` block is omitted** (see [`Program.cs`](../Program.cs)). If you provide a `SessionConfig` without a `SessionId`, it stays at `Guid.Empty` — set it explicitly. |
| `SessionCookie` | `string` | no | Opaque string passed to the game server in the allocation. LMA prints a warning if not specified. |
| `InitialPlayers` | `List<string>` | no | Initial player IDs included in the simulated allocation. |
| `Metadata` | `Dictionary<string,string>` | no | Free-form session metadata exposed to the game server. |
| `LegacyAllocationInfo` | `object` | no | Optional `ClusterManifest` (`IDictionary<string,string>`) returned to legacy game servers in heartbeat responses. Most modern GSDK-based servers can leave this unset. |

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
