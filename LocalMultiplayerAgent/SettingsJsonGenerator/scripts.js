var BuildGuId = crypto.randomUUID();
var SessionGuId = crypto.randomUUID()

/// DO NOT CHECK IN:
/// https://playfab.visualstudio.com/PlayFab/_wiki/wikis/Thunderhead/1299/Run-LocalMultiplayerAgent
/// Deep details about how each field works
/// DO NOT CHECK IN

function popAlert() {
	alert("Hello! I am an alert box!");
}

function setText(newText){
  document.getElementById("outputText").value = newText;
}

function readWriteValue(value, valueName, lmaConfig){
	if(lmaConfig) {
		lmaConfig[valueName] = value;
	}
	mirrorElement = document.getElementById(valueName + "Output");
	if(mirrorElement) {
		mirrorElement.innerHTML = value;
	}
}

function onInputChange(){
	lmaConfig = {
		// Empty containers that will hold stuff that must exist
		"AssetDetails": [{}],
		"PortMappingsList": [[{"GamePort": {}}]]
	};
	
	if(document.getElementById("OutputPlaceholders").checked) {
		lmaConfig.GameCertificateDetails = [];
		lmaConfig.SessionConfig = { "SessionCookie": null, };
		readWriteValue(SessionGuId, "SessionId", lmaConfig.SessionConfig);
		readWriteValue(BuildGuId, "BuildId", lmaConfig);
	}

	readWriteValue(document.getElementById("RunContainer").checked, "RunContainer", lmaConfig);
	if(lmaConfig.RunContainer){
		lmaConfig.ContainerStartParameters = 
		{
			"StartGameCommand": "",
			"resourcelimits": { "cpus": 1, "memorygib": 16 },
			"imagedetails": { "registry": "mcr.microsoft.com", "imagename": "playfab/multiplayer", "imagetag": "wsc-10.0.17763.973.1", "username": "", "password": "" }
		};
		readWriteValue(document.getElementById("MountPath").value, "MountPath", lmaConfig.AssetDetails[0]); // REVIEWER QUESTION: Container only?
	} else {
		lmaConfig.ProcessStartParameters = {"StartGameCommand": ""};
	}

	readWriteValue(document.getElementById("OutputFolder").value, "OutputFolder", lmaConfig);
	readWriteValue(document.getElementById("LocalFilePath").value, "LocalFilePath", lmaConfig.AssetDetails[0]);

	readWriteValue(document.getElementById("NumHeartBeatsForActivateResponse").value, "NumHeartBeatsForActivateResponse", lmaConfig);
	readWriteValue(document.getElementById("NumHeartBeatsForTerminateResponse").value, "NumHeartBeatsForTerminateResponse", lmaConfig);

	readWriteValue(document.getElementById("AgentListeningPort").value, "AgentListeningPort", lmaConfig);
	readWriteValue(document.getElementById("NodePort").value, "NodePort", lmaConfig.PortMappingsList[0][0]);
	readWriteValue(document.getElementById("GamePortNumber").value, "Number", lmaConfig.PortMappingsList[0][0].GamePort);
	readWriteValue(document.getElementById("GamePortName").value, "Name", lmaConfig.PortMappingsList[0][0].GamePort);
	readWriteValue(document.getElementById("GamePortProtocol").checked ? "TCP" : "UDP", "Protocol", lmaConfig.PortMappingsList[0][0].GamePort);
	
	setText(JSON.stringify(lmaConfig, null, 2));
}
