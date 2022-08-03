After testing their game server(s) on LMA, you may want to go ahead and deploy them.
This tool was developed exactly for this purpose; allow you go ahead to deploy your game servers.
Technically, you can also just run deployment without testing with LMA **but testing is highly recommended**

## Requirements:

- Make sure your [MultiplayerSettings.json](../MultiplayerSettings.json) is configured right
    - These parameters in MultiplayerSettings.json are not required/do not matter if you are trying to run deployment:
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

## Steps:

- Configure [build settings](./DeploymentSettings.json). Feel free to reconfigure DeploymentSettings.json to suit your needs. Example:
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

    - These are other optional parameters you could add, if needed:
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
    `.\LocalMultiplayerAgent.exe -deploy`

    - for Linux Container:
    `.\LocalMultiplayerAgent.exe -lcow -deploy`