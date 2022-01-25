import tippy from "tippy.js";
import "tippy.js/dist/tippy.css"; // optional for styling

/* START Remove when TypeScript 4.6 supports the Crypto type natively */
interface Crypto {
	randomUUID: () => string;
}
declare var crypto: Crypto;
/* END */

// Set these exactly once on page load, so they are consistent for a single session, but still random across multiple sessions
let BuildGuId = crypto.randomUUID();
let SessionGuId = crypto.randomUUID();

const WINDOWS_DEFAULT_CONTAINER_DETAILS = {
	registry: "mcr.microsoft.com",
	imagename: "playfab/multiplayer",
	imagetag: "wsc-10.0.17763.973.1",
	username: "",
	password: "",
};
const LINUX_DEFAULT_CONTAINER_DETAILS = {
	Registry: "REPLACE_WITH_CUSTOMER_ID.azurecr.io",
	ImageName: "REPLACE_WITH_CUSTOMER_IMAGE_NAME",
	ImageTag: "REPLACE_WITH_CUSTOMER_IMAGE_VERSION",
	Username: "REPLACE_WITH_CUSTOMER_USERNAME",
	Password: "REPLACE_WITH_CUSTOMER_PASSWORD",
};

// These values match an enum for RunMode in the HTML file
const RUN_MODE_WIN_PROCESS = "WinProcess";
const RUN_MODE_WIN_CONTAINER = "WinContainer";
const RUN_MODE_LINUX_CONTAINER = "LinuxContainer";

// regex strings for various validation steps
const ANTIREQUIRED_ABSOLUTE_PATH_SEARCH = ":"; // Rough attempt at identifying an apparent absolute path
const FAKE_PATH_SEARCH = "fakepath"; // New browsers spoof a fake path into JavaScript, and hide the real path
const SUGGESTED_WIN_EXTRACT_PATH_SEARCH = "C:\\\\Assets"; // Windows Process and Container modes are both suggested to use this path
const SUGGESTED_LINUX_EXTRACT_PATH_SEARCH = "/Data/"; // Linux container mode is suggested to use this root path
const REQUIRED_WIN_CONTAINER_EXTRACT_PATH_SEARCH = "C:\\\\"; // Windows Container always has exactly 1 drive, the C:\ drive

// User visible messages - Basically a string table for eventual translation if we go that far
const MSG_START_RELATIVE_PATH = "The Start Command should be a relative path into the zip file, not an absolute path.";
const MSG_START_ABSOLUTE_PATH = "The Start Command should be an absolute path that starts with the Asset Mount Path";
const MSG_START_EMPTY_PATH =
	"The Start Command should be empty (The container should launch the GSDK and Game Server directly)";
const MSG_OBSCURED_PATH =
	"Warning: This browser obscures the actual path of files. You will need to manually fix the LocalFilePath in the json";
const MSG_EXTRACT_WIN_PROCESS = "We recommended you choose C:\\Assets or a sub-folder";
const MSG_EXTRACT_WIN_CONTAINER = "Your path must start with the C:\\ drive for Windows containers";
const MSG_EXTRACT_LINUX_CONTAINER = "We recommended you choose a sub-folder of /Data";

function readWriteValue(value: any, valueName: string, lmaConfig: any) {
	if (lmaConfig) {
		lmaConfig[valueName] = value;
	}
	let mirrorElement = document.getElementById(valueName + "Output");
	if (mirrorElement) {
		mirrorElement.innerHTML = value;
	}
}

function RunAllValidations() {
	ValidateStartCommand();
	ValidateAssetZip();
	ValidateMountPath();
}

function GetElementValue(id: string): string {
	return (document.getElementById(id) as HTMLInputElement).value;
}

function ValidateStartCommand() {
	let runMode = GetElementValue("RunMode");
	let startCommand = GetElementValue("StartCommand");

	let validationMessage = "";
	if (runMode === RUN_MODE_WIN_PROCESS) {
		let isValid = startCommand.search(ANTIREQUIRED_ABSOLUTE_PATH_SEARCH) === -1;
		if (!isValid) {
			validationMessage = MSG_START_RELATIVE_PATH;
		}
	} else if (runMode === RUN_MODE_WIN_CONTAINER) {
		let mountPath = GetElementValue("MountPath");
		// Verify that the mountPath is at index zero, and thus startCommand starts with mountPath
		let isValid = startCommand.search(mountPath) === 0;
		if (!isValid) {
			validationMessage = MSG_START_ABSOLUTE_PATH;
		}
	} else if (runMode === RUN_MODE_LINUX_CONTAINER) {
		let mountPath = GetElementValue("MountPath");
		if (startCommand) {
			validationMessage = MSG_START_EMPTY_PATH;
		}
	}

	// PAUL: Put your Linux-specific fields here
	// Show the linux-docker-fields div if the user selects Linux docker containers
	(document.getElementById("linux-docker-fields") as HTMLDivElement).className =
		runMode === RUN_MODE_LINUX_CONTAINER ? "" : "hidden";

	// TODO: This could instead be a little red exclamation mark, with the validationMessage as hovertext
	(document.getElementById("StartCommandValidate") as HTMLSpanElement).innerHTML = validationMessage;
}

function ValidateAssetZip() {
	let validationMessage = "";
	let fakepathIndex = GetElementValue("LocalFilePath").search(FAKE_PATH_SEARCH);
	if (fakepathIndex != -1) {
		validationMessage = MSG_OBSCURED_PATH;
	}

	(document.getElementById("LocalFilePathValidate") as HTMLSpanElement).innerHTML = validationMessage;
}

function ValidateMountPath() {
	let validationMessage = "";
	let warningMessage = "";
	let runMode = GetElementValue("RunMode");
	let mountPath = GetElementValue("MountPath");

	if (runMode === RUN_MODE_WIN_PROCESS) {
		if (mountPath.search(SUGGESTED_WIN_EXTRACT_PATH_SEARCH) != 0) {
			warningMessage = MSG_EXTRACT_WIN_PROCESS;
		}
	} else if (runMode === RUN_MODE_WIN_CONTAINER) {
		if (mountPath.search(REQUIRED_WIN_CONTAINER_EXTRACT_PATH_SEARCH) != 0) {
			validationMessage = MSG_EXTRACT_WIN_CONTAINER;
		} else if (mountPath.search(SUGGESTED_WIN_EXTRACT_PATH_SEARCH) != 0) {
			warningMessage = MSG_EXTRACT_WIN_PROCESS;
		}
	} else if (runMode === RUN_MODE_LINUX_CONTAINER) {
		if (mountPath.search(SUGGESTED_LINUX_EXTRACT_PATH_SEARCH) != 0) {
			warningMessage = MSG_EXTRACT_LINUX_CONTAINER;
		}
	}

	// TODO: This could instead be a little red exclamation mark, with the validationMessage as hovertext
	(document.getElementById("MountPathValidate") as HTMLSpanElement).innerHTML = validationMessage;
	// TODO: This could instead be a little yellow hazard mark, with the warningMessage as hovertext
	(document.getElementById("MountPathWarning") as HTMLSpanElement).innerHTML = warningMessage;
}

function onInputChange() {
	let lmaConfig = {
		// Empty containers that will hold stuff that must exist
		AssetDetails: [{}],
		PortMappingsList: [[{ GamePort: {} }]],
	} as any;

	if ((document.getElementById("OutputPlaceholders") as HTMLInputElement).checked) {
		lmaConfig.GameCertificateDetails = [];
		lmaConfig.SessionConfig = { SessionCookie: null };
		lmaConfig.DeploymentMetadata = { Environment: "LOCAL", FeaturesEnabled: "List,Of,Features,Enabled" };
		let initialPlayersArray = GetElementValue("InitialPlayers").split(",");
		for (let i = 0; i < initialPlayersArray.length; i++) {
			initialPlayersArray[i] = initialPlayersArray[i].trim();
		}

		readWriteValue(SessionGuId, "SessionId", lmaConfig.SessionConfig);
		readWriteValue(initialPlayersArray, "InitialPlayers", lmaConfig.SessionConfig);
		readWriteValue(GetElementValue("TitleId"), "TitleId", lmaConfig);
		readWriteValue(GetElementValue("Region"), "Region", lmaConfig);
		readWriteValue(BuildGuId, "BuildId", lmaConfig);
	}

	let startCommand = GetElementValue("StartCommand");
	let runMode = GetElementValue("RunMode");

	readWriteValue(runMode !== RUN_MODE_WIN_PROCESS, "RunMode", lmaConfig);
	if (runMode === RUN_MODE_WIN_PROCESS) {
		lmaConfig.ProcessStartParameters = { StartGameCommand: startCommand };
	} else {
		lmaConfig.ContainerStartParameters = {
			StartGameCommand: startCommand,
			resourcelimits: { cpus: 1, memorygib: 16 },
		};

		if (runMode === RUN_MODE_WIN_CONTAINER) {
			lmaConfig.ContainerStartParameters.imagedetails = WINDOWS_DEFAULT_CONTAINER_DETAILS;
		} else if (runMode === RUN_MODE_LINUX_CONTAINER) {
			lmaConfig.ContainerStartParameters.imagedetails = LINUX_DEFAULT_CONTAINER_DETAILS;
		}

		readWriteValue(GetElementValue("MountPath"), "MountPath", lmaConfig.AssetDetails[0]);
		readWriteValue(GetElementValue("GamePortNumber"), "Number", lmaConfig.PortMappingsList[0][0].GamePort);
	}

	readWriteValue(GetElementValue("OutputFolder"), "OutputFolder", lmaConfig);
	readWriteValue(GetElementValue("LocalFilePath"), "LocalFilePath", lmaConfig.AssetDetails[0]);

	readWriteValue(GetElementValue("NumHeartBeatsForActivateResponse"), "NumHeartBeatsForActivateResponse", lmaConfig);
	readWriteValue(
		GetElementValue("NumHeartBeatsForTerminateResponse"),
		"NumHeartBeatsForTerminateResponse",
		lmaConfig
	);

	readWriteValue(GetElementValue("AgentListeningPort"), "AgentListeningPort", lmaConfig);
	readWriteValue(GetElementValue("NodePort"), "NodePort", lmaConfig.PortMappingsList[0][0]);
	readWriteValue(GetElementValue("GamePortName"), "Name", lmaConfig.PortMappingsList[0][0].GamePort);
	readWriteValue(GetElementValue("GamePortProtocol"), "Protocol", lmaConfig.PortMappingsList[0][0].GamePort);

	(document.getElementById("outputText") as HTMLTextAreaElement).value = JSON.stringify(lmaConfig, null, 2);

	RunAllValidations();
}

const allInputElementsArray = Array.prototype.slice.call(document.getElementsByTagName("input"), 0);
const allSelectElementsArray = Array.prototype.slice.call(document.getElementsByTagName("select"), 0);

allInputElementsArray.forEach((inputElement: HTMLInputElement) => {
	inputElement.addEventListener("change", () => {
		onInputChange();
	});
});

allSelectElementsArray.forEach((selectElement: HTMLInputElement) => {
	selectElement.addEventListener("change", () => {
		onInputChange();
	});
});

onInputChange();

tippy("[data-tippy-content]");
