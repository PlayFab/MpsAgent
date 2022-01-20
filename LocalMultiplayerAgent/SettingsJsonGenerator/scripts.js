// Set these exactly once on page load, so they are consistent for a single session, but still random across multiple sessions
let BuildGuId = crypto.randomUUID();
let SessionGuId = crypto.randomUUID()

function readWriteValue(value, valueName, lmaConfig){
    if(lmaConfig){
        lmaConfig[valueName] = value;
    }
    let mirrorElement = document.getElementById(valueName + "Output");
    if(mirrorElement){
        mirrorElement.innerHTML = value;
    }
}

function RunAllValidations(){
    ValidateStartCommand();
    ValidateAssetZip();
    ValidateMountPath();
}

function ValidateStartCommand(){
    let runMode = document.getElementById("RunContainer").value;
    let startCommand = document.getElementById("StartCommand").value;

    let validationMessage = "";
    if (runMode == "WinProcess"){
        // Very lousy first-attempt at identifying an apparent absolute path
        let isValid = (startCommand.search(":") == -1);
        if (!isValid){
            validationMessage = "The Start Command should be a relative path into the zip file, not an absolute path.";
        }
    }else if(runMode == "WinContainer"){
        let mountPath = document.getElementById("MountPath").value;
        // Verify that the mountPath is at index zero, and thus startCommand starts with mountPath
        let isValid = (startCommand.search(mountPath) == 0);
        if (!isValid){
            validationMessage = "The Start Command should be an absolute path that starts with the Asset Mount Path";
        }
    }else if(runMode == "LinuxContainer"){
        let mountPath = document.getElementById("MountPath").value;
        if (startCommand){
            validationMessage = "The Start Command should be empty (The container should launch the GSDK and Game Server)";
        }
    }

    // TODO: This could instead be a little red exclamation mark, with the validationMessage as hovertext
    document.getElementById("StartCommandValidate").innerHTML = validationMessage;
}

function ValidateAssetZip(){
    let validationMessage = "";
    let fakepathIndex = document.getElementById("LocalFilePath").value.search("fakepath");
    if (fakepathIndex != -1){
        validationMessage = "Warning: This browser obscures the actual path of files. You will need to manually fix the LocalFilePath in the json"
    }
        
    document.getElementById("LocalFilePathValidate").innerHTML = validationMessage;
}

function ValidateMountPath(){
    let validationMessage = "";
    let warningMessage = "";
    let runMode = document.getElementById("RunContainer").value;
    let mountPath = document.getElementById("MountPath").value;

    if (runMode == "WinProcess"){
        if(mountPath.search("C:\\\\Assets") != 0){
            warningMessage = "It is recommended that you choose C:\\Assets or a sub-folder";
        }
    }else if(runMode == "WinContainer"){
        if(mountPath.search("C:\\\\") != 0){
            validationMessage = "Your path must start with the C:\\ drive for Windows containers";
        } else if(mountPath.search("C:\\\\Assets") != 0){
            warningMessage = "It is recommended that you choose C:\\Assets or a sub-folder";
        }
    }else if(runMode == "LinuxContainer"){
        if(mountPath.search("/Data/") != 0){
            warningMessage = "It is recommended that you choose a sub-folder of /Data";
        }
    }
        
    document.getElementById("MountPathValidate").innerHTML = validationMessage;
    document.getElementById("MountPathWarning").innerHTML = warningMessage;
}

function onInputChange(){
    let lmaConfig = {
        // Empty containers that will hold stuff that must exist
        "AssetDetails": [{}],
        "PortMappingsList": [[{"GamePort": {}}]]
    };

    if(document.getElementById("OutputPlaceholders").checked){
        lmaConfig.GameCertificateDetails = [];
        lmaConfig.SessionConfig = { "SessionCookie": null, };
        lmaConfig.DeploymentMetadata = { "Environment": "LOCAL", "FeaturesEnabled": "List,Of,Features,Enabled" };
        let initialPlayersArray = document.getElementById("InitialPlayers").value.split(',');
        for (let i = 0; i < initialPlayersArray.length; i++){
            initialPlayersArray[i]=initialPlayersArray[i].trim();
        }

        readWriteValue(SessionGuId, "SessionId", lmaConfig.SessionConfig);
        readWriteValue(initialPlayersArray, "InitialPlayers", lmaConfig.SessionConfig);
        readWriteValue(document.getElementById("TitleId").value, "TitleId", lmaConfig);
        readWriteValue(document.getElementById("Region").value, "Region", lmaConfig);
        readWriteValue(BuildGuId, "BuildId", lmaConfig);
    }

    let startCommand = document.getElementById("StartCommand").value;
    let runMode = document.getElementById("RunContainer").value;

    readWriteValue(runMode != "WinProcess", "RunContainer", lmaConfig);
    if(runMode == "WinProcess")
    {
        lmaConfig.ProcessStartParameters = {"StartGameCommand": startCommand};
    }else{
        lmaConfig.ContainerStartParameters =
        {
            "StartGameCommand": startCommand,
            "resourcelimits": { "cpus": 1, "memorygib": 16 },
            "imagedetails": { "registry": "mcr.microsoft.com", "imagename": "playfab/multiplayer", "imagetag": "wsc-10.0.17763.973.1", "username": "", "password": "" }
        };
        readWriteValue(document.getElementById("MountPath").value, "MountPath", lmaConfig.AssetDetails[0]);
        readWriteValue(document.getElementById("GamePortNumber").value, "Number", lmaConfig.PortMappingsList[0][0].GamePort);
    }


    readWriteValue(document.getElementById("OutputFolder").value, "OutputFolder", lmaConfig);
    readWriteValue(document.getElementById("LocalFilePath").value, "LocalFilePath", lmaConfig.AssetDetails[0]);

    readWriteValue(document.getElementById("NumHeartBeatsForActivateResponse").value, "NumHeartBeatsForActivateResponse", lmaConfig);
    readWriteValue(document.getElementById("NumHeartBeatsForTerminateResponse").value, "NumHeartBeatsForTerminateResponse", lmaConfig);

    readWriteValue(document.getElementById("AgentListeningPort").value, "AgentListeningPort", lmaConfig);
    readWriteValue(document.getElementById("NodePort").value, "NodePort", lmaConfig.PortMappingsList[0][0]);
    readWriteValue(document.getElementById("GamePortName").value, "Name", lmaConfig.PortMappingsList[0][0].GamePort);
    readWriteValue(document.getElementById("GamePortProtocol").value, "Protocol", lmaConfig.PortMappingsList[0][0].GamePort);

    document.getElementById("outputText").value = JSON.stringify(lmaConfig, null, 2);
    
    RunAllValidations();
}
