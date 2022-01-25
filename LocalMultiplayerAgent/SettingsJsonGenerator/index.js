(()=>{"use strict";let e=crypto.randomUUID(),t=crypto.randomUUID();const a={registry:"mcr.microsoft.com",imagename:"playfab/multiplayer",imagetag:"wsc-10.0.17763.973.1",username:"",password:""},n={Registry:"REPLACE_WITH_CUSTOMER_ID.azurecr.io",ImageName:"REPLACE_WITH_CUSTOMER_IMAGE_NAME",ImageTag:"REPLACE_WITH_CUSTOMER_IMAGE_VERSION",Username:"REPLACE_WITH_CUSTOMER_USERNAME",Password:"REPLACE_WITH_CUSTOMER_PASSWORD"},o="WinProcess",r="WinContainer",s="LinuxContainer",i="C:\\\\Assets",l="It is recommended that you choose C:\\Assets or a sub-folder";function m(e,t,a){a&&(a[t]=e);let n=document.getElementById(t+"Output");n&&(n.innerHTML=e)}function u(e){return document.getElementById(e).value}function d(){let d={AssetDetails:[{}],PortMappingsList:[[{GamePort:{}}]]};if(document.getElementById("OutputPlaceholders").checked){d.GameCertificateDetails=[],d.SessionConfig={SessionCookie:null},d.DeploymentMetadata={Environment:"LOCAL",FeaturesEnabled:"List,Of,Features,Enabled"};let a=u("InitialPlayers").split(",");for(let e=0;e<a.length;e++)a[e]=a[e].trim();m(t,"SessionId",d.SessionConfig),m(a,"InitialPlayers",d.SessionConfig),m(u("TitleId"),"TitleId",d),m(u("Region"),"Region",d),m(e,"BuildId",d)}let c=u("StartCommand"),h=u("RunMode");m(h!==o,"RunMode",d),h==o?d.ProcessStartParameters={StartGameCommand:c}:(d.ContainerStartParameters={StartGameCommand:c,resourcelimits:{cpus:1,memorygib:16}},h==r?d.ContainerStartParameters.imagedetails=a:h==s&&(d.ContainerStartParameters.imagedetails=n),m(u("MountPath"),"MountPath",d.AssetDetails[0]),m(u("GamePortNumber"),"Number",d.PortMappingsList[0][0].GamePort)),m(u("OutputFolder"),"OutputFolder",d),m(u("LocalFilePath"),"LocalFilePath",d.AssetDetails[0]),m(u("NumHeartBeatsForActivateResponse"),"NumHeartBeatsForActivateResponse",d),m(u("NumHeartBeatsForTerminateResponse"),"NumHeartBeatsForTerminateResponse",d),m(u("AgentListeningPort"),"AgentListeningPort",d),m(u("NodePort"),"NodePort",d.PortMappingsList[0][0]),m(u("GamePortName"),"Name",d.PortMappingsList[0][0].GamePort),m(u("GamePortProtocol"),"Protocol",d.PortMappingsList[0][0].GamePort),document.getElementById("outputText").value=JSON.stringify(d,null,2),function(){let e=u("RunMode"),t=u("StartCommand"),a="";if(e==o)-1==t.search(":")||(a="The Start Command should be a relative path into the zip file, not an absolute path.");else if(e==r){let e=u("MountPath");0==t.search(e)||(a="The Start Command should be an absolute path that starts with the Asset Mount Path")}else e==s&&(u("MountPath"),t&&(a="The Start Command should be empty (The container should launch the GSDK and Game Server directly)"));document.getElementById("StartCommandValidate").innerHTML=a}(),function(){let e="";-1!=u("LocalFilePath").search("fakepath")&&(e="Warning: This browser obscures the actual path of files. You will need to manually fix the LocalFilePath in the json"),document.getElementById("LocalFilePathValidate").innerHTML=e}(),function(){let e="",t="",a=u("RunMode"),n=u("MountPath");a==o?0!=n.search(i)&&(t=l):a==r?0!=n.search("C:\\\\")?e="Your path must start with the C:\\ drive for Windows containers":0!=n.search(i)&&(t=l):a==s&&0!=n.search("/Data/")&&(t="It is recommended that you choose a sub-folder of /Data"),document.getElementById("MountPathValidate").innerHTML=e,document.getElementById("MountPathWarning").innerHTML=t}()}const c=Array.prototype.slice.call(document.getElementsByTagName("input"),0),h=Array.prototype.slice.call(document.getElementsByTagName("select"),0);c.forEach((e=>{e.addEventListener("change",(()=>{d()}))})),h.forEach((e=>{e.addEventListener("change",(()=>{d()}))})),d()})();