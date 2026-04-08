# Linux Support

You can use LocalMultiplayerAgent on Linux to debug your game servers using either **container mode** (Linux containers) or **process mode** (bare processes).

## Container Mode (Linux Containers on Linux)

Container mode runs your game server inside a Docker container natively on Linux.

### Prerequisites

- [Install Docker Engine](https://docs.docker.com/engine/install/) on your Linux machine
- Make sure Docker is running (`sudo systemctl start docker`)
- Your game server image can be published on a container registry or can be locally built

### Setup

1. Download the latest version of LocalMultiplayerAgent for Linux (linux-x64) from the [Releases](https://github.com/PlayFab/MpsAgent/releases/) page on GitHub

2. Run the `setup_linux.sh` script which will create a Docker network called "playfab":

```bash
chmod +x setup_linux.sh
./setup_linux.sh
```

3. Configure your *MultiplayerSettings.json* file. Below you can see a sample, included in `MultiplayerSettingsLinuxContainersOnLinuxSample.json`:

```json
{
  "RunContainer": true,
  "OutputFolder": "",
  "NumHeartBeatsForActivateResponse": 10,
  "NumHeartBeatsForTerminateResponse": 60,
  "TitleId": "",
  "BuildId": "00000000-0000-0000-0000-000000000000",
  "Region": "WestUs",
  "AgentListeningPort": 56001,
  "ContainerStartParameters": {
    "ImageDetails": {
      "Registry": "mydockerregistry.io",
      "ImageName": "mygame",
      "ImageTag": "0.1",
      "Username": "",
      "Password": ""
    }
  },
  "PortMappingsList": [
    [
      {
        "NodePort": 56100,
        "GamePort": {
          "Name": "gameport",
          "Number": 7777,
          "Protocol": "TCP"
        }
      }
    ]
  ],
  "DeploymentMetadata": {
    "Environment": "LOCAL",
    "FeaturesEnabled": "List,Of,Features,Enabled"
  },
  "SessionConfig": {
    "SessionId": "ba67d671-512a-4e7d-a38c-2329ce181946",
    "SessionCookie": null,
    "InitialPlayers": [ "Player1", "Player2" ]
  }
}
```

> Notes:
> 1. Set `RunContainer` to true for container mode.
> 2. Modify `ImageDetails` with your game server Docker image details. The image may be built locally (using [docker build](https://docs.docker.com/engine/reference/commandline/build/) command) or be hosted in a remote container registry.
> 3. `StartGameCommand` and `AssetDetails` are optional. You don't normally use them when you use a Docker container since all game assets + start game server command can be packaged in the corresponding [Dockerfile](https://docs.docker.com/engine/reference/builder/). `DeploymentMetadata` is also optional and can contain up to 30 dictionary mappings of custom metadata to be passed to the container as the `buildMetadata` field.

4. Run LocalMultiplayerAgent:

```bash
./LocalMultiplayerAgent
```

The agent will automatically detect that it is running on Linux and configure itself for Linux game servers.

## Process Mode (Bare Processes on Linux)

Process mode runs your game server as a native Linux process without Docker.

### Prerequisites

- Your game server must be a Linux executable
- Docker is **not** required for process mode

### Setup

1. Download the latest version of LocalMultiplayerAgent for Linux (linux-x64) from the [Releases](https://github.com/PlayFab/MpsAgent/releases/) page on GitHub

2. Configure your *MultiplayerSettings.json* file. Below you can see a sample, included in `MultiplayerSettingsLinuxProcessOnLinuxSample.json`:

```json
{
  "RunContainer": false,
  "OutputFolder": "",
  "NumHeartBeatsForActivateResponse": 10,
  "NumHeartBeatsForTerminateResponse": 60,
  "TitleId": "",
  "BuildId": "00000000-0000-0000-0000-000000000000",
  "Region": "WestUs",
  "AgentListeningPort": 56001,
  "AssetDetails": [
    {
      "MountPath": "",
      "LocalFilePath": "<path_to_game_server_package>"
    }
  ],
  "PortMappingsList": [
    [
      {
        "NodePort": 56100,
        "GamePort": {
          "Name": "gameport",
          "Number": 7777,
          "Protocol": "TCP"
        }
      }
    ]
  ],
  "ProcessStartParameters": {
    "StartGameCommand": "<your_game_server_executable>"
  },
  "SessionConfig": {
    "SessionId": "ba67d671-512a-4e7d-a38c-2329ce181946",
    "SessionCookie": null,
    "InitialPlayers": [ "Player1", "Player2" ]
  }
}
```

> Notes:
> 1. Set `RunContainer` to false for process mode.
> 2. `AssetDetails` is required for process mode. Set `LocalFilePath` to the path of your game server zip package.
> 3. `StartGameCommand` in `ProcessStartParameters` should be the path to your game server executable (relative to the extracted assets).

3. Run LocalMultiplayerAgent:

```bash
./LocalMultiplayerAgent
```

## Building from Source

```bash
git clone https://github.com/PlayFab/MpsAgent.git
cd ./MpsAgent/LocalMultiplayerAgent
dotnet publish --runtime linux-x64 -c Release -o LocalMultiplayerAgentPublishFolder -p:PublishSingleFile=true --self-contained true
```

## Troubleshooting

- If you encounter permission issues running LocalMultiplayerAgent, make it executable: `chmod +x ./LocalMultiplayerAgent`
- For container mode, ensure your user is in the `docker` group or use `sudo`: `sudo usermod -aG docker $USER`
- If the Docker socket is not accessible, verify Docker is running: `sudo systemctl status docker`
