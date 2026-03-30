# Development instructions for LocalMultiplayerAgent

LocalMultiplayerAgent runs on **Windows** and **MacOS (Apple Silicon) (beta)**. Please see the start up scripts for each platform for setting up firewalls and installing additional dependencies.

Instructions for using LocalMultiplayerAgent can be found in the [LocalMultiplayerAgent documentation](https://learn.microsoft.com/en-us/gaming/playfab/features/multiplayer/servers/localmultiplayeragent/local-multiplayer-agent-overview).

## Build For Windows:

In a VS2022 developer cmd prompt:
dotnet publish VMAgent.csproj -c release -o outputFolder --runtime win10-x64

The "outputFolder" in the directory containing the csproj file will have the dlls and exe.

## Build For Ubuntu: 

In a terminal, navigate to the directory containing the csproj file and run the following command:

```bash
dotnet publish VMAgent.csproj -c release -o outputFolder 
```

The "outputFolder" in the directory containing the csproj file will have the dlls and executable.
Once the files are copied over, the LocalMultiplayerAgent file will need to be converted to executable - chmod +X LocalMultiplayerAgent

## Build For MacOS (Apple Silicon):

In a terminal, navigate to the directory containing the csproj file and run the following command:

```bash
dotnet publish LocalMultiplayerAgent.csproj --runtime osx-arm64 -c Release -o outputFolder -p:PublishSingleFile=true --self-contained true
```

The "outputFolder" in the directory containing the csproj file will have the executable.

MacOS only supports Linux containers. Before running, make sure Docker Desktop for Mac is installed and run the setup script:

```bash
chmod +x setup_macos.sh
./setup_macos.sh
```

Configure `MultiplayerSettings.json` with `RunContainer: true` (see `MultiplayerSettingsLinuxContainersOnMacOSSample.json` for a reference configuration). The agent automatically detects MacOS and configures for Linux containers — no additional flags are needed.

> **Troubleshooting:** If MacOS prevents you from running LocalMultiplayerAgent, run `xattr -d com.apple.quarantine ./LocalMultiplayerAgent` to remove the quarantine attribute.

For detailed instructions, see [Linux Containers on MacOS](../macos.md).

## MultiplayerSettings.json

The [MultiplayerSettings.json generator tool](./SettingsJsonGenerator/README.md) can assist you with configuring your `MultiplayerSettings.json` file, and configuring it for your server.
The [LMA Build Tool](./BuildTool/readme.md) can assist you with configuring your [`BuildSettings.json`](./BuildTool/BuildSettings.json) file for creating builds through LocalMultiplayerAgent.
