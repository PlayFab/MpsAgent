# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

<#
.DESCRIPTION
This script sets up your computer to run the Playfab Multiplayer Server LocalMultiplayerAgent

.PARAMETER AgentPort
When specified, override the port that will be used by the agent.

#>
param
(
    [Parameter(Mandatory = $false)]
    [int] $AgentPort = 56001
)

$playFabVmAgentString = "PlayFabVmAgent";
function CreateDockerNetwork()
{
    $existingDockerNetwork = docker network ls | Select-String "playfab";
    if ($existingDockerNetwork -eq $null)
    {            
        $start = 19;
        $end = 22;
        $success = $false;
        for ($i = $start; $i -lt $end; $i++)
        {
            $newSubnet = "172.$i.0.0/16"
            $newGateway = "172.$i.0.11"
            Write-Host "Creating new subnet $newSubnet with gateway $newGateway"

            docker network create playfab --driver nat --subnet $newSubnet --gateway $newGateway

            if ($LastExitCode -ne 0)
            {
                Write-Host "Network creation for $newSubnet / $newGateway failed."
                continue;
            }
            else
            {
                $success = $true;
            }

            break;
        }

        if ($success -eq $false)
        {
            Write-Host "All attempts to create network failed.";
            exit 1;
        }
    }
}

function AddFirewallRules
{
    Param(
        [int] $firewallPort
    )

    $existingFirewallRule = Get-NetFirewallRule -DisplayName "$playFabVmAgentString-$firewallPort"  -ErrorAction SilentlyContinue;
    if ($existingFirewallRule -eq $null)
    {
        New-NetFirewallRule -DisplayName "$playFabVmAgentString-$firewallPort" -Name "$playFabVmAgentString-$firewallPort" -Enabled True -Direction Inbound -LocalPort $firewallPort -Protocol TCP -Action Allow;
    }
}

# Make sure any errors by default would stop the execution of the script and show as start task failure.
# This can be overriden explicitly for cmdlets where we don't care about the error.
$ErrorActionPreference = "Stop"

# Start-Service is idempotent. If the service is already running, it is ignored without error.
Start-Service com.docker.service
CreateDockerNetwork;

Write-Host "Verifying firewall rules."
AddFirewallRules $AgentPort;

# Pull the docker image for playfab windows container image.
Write-Host "Pulling docker image.."
docker pull mcr.microsoft.com/playfab/multiplayer:wsc-10.0.17763.2458

