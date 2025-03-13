# Development instructions for LocalMultiplayerAgent

Please see start up script for Linux and Windows for setting up firewalls and installing additional dependencies.

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

## MultiplayerSettings.json

The [MultiplayerSettings.json generator tool](./SettingsJsonGenerator/README.md) can assist you with configuring your `MultiplayerSettings.json` file, and configuring it for your server.
The [LMA Build Tool](./BuildTool/readme.md) can assist you with configuring your [`BuildSettings.json`](./BuildTool/BuildSettings.json) file for creating builds through LocalMultiplayerAgent.
