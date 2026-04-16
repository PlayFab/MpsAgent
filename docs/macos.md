# Linux Containers on MacOS (beta)

You can use LocalMultiplayerAgent to debug your Linux game server by running it on a container on MacOS using [Docker Desktop for Mac](https://docs.docker.com/desktop/install/mac-install/). LocalMultiplayerAgent automatically detects MacOS and configures itself for Linux containers — no additional flags are needed.

To run your containerized Linux game servers on MacOS, you'll need to perform the following steps:

- Download the latest version of LocalMultiplayerAgent for MacOS (osx-arm64) from the [Releases](https://github.com/PlayFab/MpsAgent/releases/) page on GitHub
- [Install Docker Desktop for Mac](https://docs.docker.com/desktop/install/mac-install/)
- Make sure Docker Desktop is running
- Your game server image can be published on a container registry or can be locally built
- Run the `setup_macos.sh` script which will create a Docker network called "playfab":

```bash
chmod +x setup_macos.sh
./setup_macos.sh
```

- Properly configure your *MultiplayerSettings.json* file. Below you can see a sample, included in `Samples/MultiplayerSettingsLinuxContainersOnMacOSSample.json`:

```json
{
  "RunContainer": true,
  "ForcePullContainerImageFromRegistry": true,
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

> This file is also included in the latest LocalMultiplayerAgent zip package. Some notes:
> 1. You must set `RunContainer` to true. MacOS only supports Linux containers.
> 2. Modify `ImageDetails` with your game server Docker image details. The image may be built locally (using [docker build](https://docs.docker.com/engine/reference/commandline/build/) command) or be hosted in a remote container registry.
> 3. If your image is hosted in a remote container registry, set `ForcePullContainerImageFromRegistry` to `true` so the agent pulls it before starting. If your image is locally built, set it to `false` (or omit it) to skip the pull.
> 4. `StartGameCommand` and `AssetDetails` are optional. You don't normally use them when you use a Docker container since all game assets + start game server command can be packaged in the corresponding [Dockerfile](https://docs.docker.com/engine/reference/builder/). `DeploymentMetadata` is also optional and can contain up to 30 dictionary mappings of custom metadata to be passed to the container as the `buildMetadata` field.
> 5. Process mode (bare process without containers) is not supported on MacOS. You must use `RunContainer: true`.

- After you perform all the previous steps, you can then run LocalMultiplayerAgent:

```bash
./LocalMultiplayerAgent
```

The agent will automatically detect that it is running on MacOS and configure itself for Linux containers. No additional command-line flags are needed.

## Troubleshooting

If MacOS prevents you from running LocalMultiplayerAgent, run the following command to remove the quarantine attribute:

```bash
xattr -d com.apple.quarantine ./LocalMultiplayerAgent
```
