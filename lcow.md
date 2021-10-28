# Linux Containers on Windows

You can use LocalMultiplayerAgent to debug your Linux game server by running it on a container in Windows using [Docker for Windows](https://docs.docker.com/docker-for-windows/). You can see more information abour running Linux containers on Windows [here](https://docs.microsoft.com/en-us/virtualization/windowscontainers/deploy-containers/linux-containers). If you are new to the container world, you can check an intro [here](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/container-docker-introduction/). In essense, all you need to is run the agent with the *-lcow* parameter as well as properly configure your *LocalMultiplayerSettings.json* file.

To run your containerized Linux game servers on Windows, you'll need to perform the following steps:

- Download latest version of LocalMultiplayerAgent from the [Releases](https://github.com/PlayFab/MpsAgent/releases/) page on GitHub
- [Install Docker Desktop on Windows](https://docs.docker.com/docker-for-windows/install/)
- Make sure it's running [Linux Containers](https://docs.docker.com/docker-for-windows/#switch-between-windows-and-linux-containers)
- You should mount one of your hard drives, instructions [here](https://docs.docker.com/docker-for-windows/#file-sharing)
- Your game server image can be published on a container registry or can be locally built.
- You should run `SetupLinuxContainersOnWindows.ps1` Powershell file which will create a Docker network called "PlayFab"
- You should properly configure your *LocalMultiplayerSettings.json* file. Below you can see a sample, included in `MultiplayerSettingsLinuxContainersOnWindowsSample.json`:

```json
{
    "RunContainer": true,
    "OutputFolder": "C:\\output\\UnityServerLinux",
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
                    "Name": "game_port",
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
> 1. You must set `RunContainer` to true. This is required for Linux game servers.
> 2. Modify `imageDetails` with your game server docker image details. Image may be built locally (using [docker build](https://docs.docker.com/engine/reference/commandline/build/) command) or be hosted in a remote container registry.
> 3. `StartGameCommand` and `AssetDetails` are optional. You don't normally use them when you use a Docker container since all game assets + start game server command can be packaged in the corresponding [Dockerfile](https://docs.docker.com/engine/reference/builder/). `DeploymentMetadata` is also optional and can contain up to 30 dictonary mappings of custom metadata to be passed to the container as the `buildMetadata` field.
> 4. Last, but definitely not least, pay attention to the casing on your `OutputFolder` variable, since Linux containers are case sensitive. If casing is wrong, you might see a Docker exception similar to *error while creating mount source path '/host_mnt/c/output/UnityServerLinux/PlayFabVmAgentOutput/2020-01-30T12-47-09/GameLogs/a94cfbb5-95a4-480f-a4af-749c2d9cf04b': mkdir /host_mnt/c/output: file exists*

- After you perform all the previous steps, you can then run the LocalMultiPlayerAgent with the command `LocalMultiplayerAgent.exe -lcow` (lcow stands for *Linux Containers On Windows*)