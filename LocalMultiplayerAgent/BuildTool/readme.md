After testing your game server(s) on Local Multiplayer Agent, you may want to go ahead and deploy them to PlayFab Multiplayer Servers.
This tool was developed exactly for this purpose; allows you go ahead to deploy your game servers to PlayFab Multiplayer Servers after testing with Local Multiplayer Agent.
Technically, you can just run game server deployment without testing with LMA **but testing is highly recommended**

## Requirements:
- Set your [PlayFab Title Secret Key](https://docs.microsoft.com/en-us/gaming/playfab/gamemanager/secret-key-management) as a **User Environment Variable**
    - Command Prompt
    `setx PF_SECRET "[variable value]"`

    - Powershell
    `[Environment]::SetEnvironmentVariable("PF_SECRET","[variable value]","User")`

- Make sure your [MultiplayerSettings.json](../MultiplayerSettings.json) is configured right
    - [Windows Process](https://docs.microsoft.com/en-us/gaming/playfab/features/multiplayer/servers/localmultiplayeragent/run-process-based-gameserver):
        `RunContainer: false
        "ProcessStartParameters": {
            "StartGameCommand": "<your_game_server_exe>"
        }
        ...`
    - [Windows Container](https://docs.microsoft.com/en-us/gaming/playfab/features/multiplayer/servers/localmultiplayeragent/run-container-gameserver):
        `RunContainer: true
        "ContainerStartParameters": {
            "StartGameCommand": "C:\\Assets\\<your_game_server_exe>",
            "ResourceLimits": {
                "Cpus": 0,
                "MemoryGib": 0
            },
            "ImageDetails": {
                "Registry": "mcr.microsoft.com",
                "ImageName": "playfab/multiplayer",
                "ImageTag": "wsc-10.0.17763.2458",
                "Username": "",
                "Password": ""
             }
        }
        ...`
    - [Linux Container](https://docs.microsoft.com/en-us/gaming/playfab/features/multiplayer/servers/localmultiplayeragent/run-container-gameserver):
        `RunContainer: true
        "ContainerStartParameters": {
            "StartGameCommand": "C:\\Assets\\<your_game_server_exe>",
            "ResourceLimits": {
                "Cpus": 0,
                "MemoryGib": 0
            },
            "ImageDetails": {
                "Registry": "mydockerregistry.io",
                "ImageName": "mygame",
                "ImageTag": "0.1",
                "Username": "",
                "Password": ""
             }
        }
        ...`

    - These parameters in MultiplayerSettings.json are not required if you are only trying to create a build:
    `  
    "OutputFolder": "",
    "NumHeartBeatsForActivateResponse": 5,
    "NumHeartBeatsForTerminateResponse": 10,
    "AgentListeningPort": 56001,
    "BuildId": "",
    "Region": "",
    "SessionConfig": {
        "SessionId": "ba67d671-512a-4e7d-a38c-2329ce181946",
        "SessionCookie": null,
        "InitialPlayers": [ "Player1", "Player2" ]
    }
    `

    - Since Linux Process is currently not supported on Local Multiplayer Agent, so users cannot create a build with this build tool

## Steps:

- Configure [build settings](./BuildSettings.json). Feel free to reconfigure BuildSettings.json to suit your needs. Example:
`
{
    "BuildName": "MyGameBuild",
    "VmSize": "Standard_D2_v2",
    "MultiplayerServerCountPerVm": 2,
    "RegionConfigurations": [
        {
            "Region": "EastUs",
            "MaxServers": 5,
            "StandbyServers": 1
        }
    ]
}
`

    - These are other optional parameters for your `BuildSettings.json` you could add, if needed:
    `
    "AreAssetsReadonly": false,
    "UseStreamingForAssetDownloads": false,
    "WindowsCrashDumpConfiguration": {  // WindowsCrashDumpConfiguration is an additional parameter you can add for Windows Container build
        "IsEnabled": true,
        "DumpType": 0,
        "CustomDumpFlags": 6693
        }
    `

- Run 
    - for Windows Process/Container:
    `.\LocalMultiplayerAgent.exe -build`

    - for Linux Container:
    `.\LocalMultiplayerAgent.exe -lcow -build`


## After running
- You should get a log message saying your build was successfully created, or an error message if it failed
- Login to your [PlayFab developer account](https://developer.playfab.com/en-us/login) 
- Navigate to the Multiplayer/Servers tab of the title under which you created your build to verify that your build was created successfully