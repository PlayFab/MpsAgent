![CI workflow](https://github.com/playfab/MpsAgent/actions/workflows/main.yml/badge.svg)
[![Software License](https://img.shields.io/badge/license-MIT-brightgreen.svg?style=flat-square)](LICENSE)

# MpsAgent

## Overview

This repository contains source code for the following projects:

- LocalMultiplayerAgent

An executable that mimics PlayFab Multiplayer Servers (MPS) operations to aid in local debugging. See [PlayFab documentation - LocalMultiplayerAgent Overview](https://docs.microsoft.com/en-us/gaming/playfab/features/multiplayer/servers/localmultiplayeragent/local-multiplayer-agent-overview) to learn how to debug your game servers using LocalMultiplayerAgent. If you want to develop for Linux Containers on Windows, check out [this document](lcow.md).

> This repository replaces and deprecates the project in [this GitHub repo](https://github.com/PlayFab/LocalMultiplayerAgent). The executable produced by that project was called `MockAgent`. This has been renamed to `LocalMultiplayerAgent` for consistency. 

- AgentInterfaces and VmAgent.Core

These two helper libraries are used by LocalMultiplayerAgent and the production [Azure PlayFab Multiplayer Servers service](https://docs.microsoft.com/en-us/gaming/playfab/features/multiplayer/servers/).

> If you want to test your Linux game servers on Kubernetes, check out [our new developer tool, thundernetes](https://github.com/PlayFab/thundernetes).

## Building

1. Have [.NET Core](https://dotnet.microsoft.com/download) installed.

2. [Download a release](https://github.com/PlayFab/MpsAgent/releases) and unzip the parent `LocalMultiplayerAgentPublish` dir under the root `MpsAgent/` parent dir.

3. Run the following in Powershell (without admin):

```bash
git clone https://github.com/PlayFab/MpsAgent.git
cd ./MpsAgent/LocalMultiplayerAgent 
# replace the LocalMultiplayerAgentPublish with the folder of your choice
dotnet publish --runtime win-x64 -c Release -o LocalMultiplayerAgentPublishFolder -p:PublishSingleFile=true --self-contained true
# you can read here about .NET publish CLI options https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish
```

## Contributing

We are more than happy to accept external contributions! If you want to propose a small code change to LocalMultiplayerAgent, feel free to open a Pull Request directly. If you plan to do a bigger change to LocalMultiplayerAgent, it would be better if you open an issue describing your proposed design in order to get feedback from project maintainers.

For AgentInterfaces and VmAgent.Core, we'd recommend to open an issue describing your proposal/idea. Since these projects are used by the service currently in production, we'd need to do additional validation and testing before accepting external contributions.

Furthermore, the YAML files (AgentInterfaces.yml, LocalMultiplayerAgent.yml, VmAgent.Core.yml) should not be altered since they are used by our internal Azure DevOps pipelines.

## Contact Us

We love to hear from our developer community!
Do you have ideas on how we can make our products and services better?

Our Developer Success Team can assist with answering any questions as well as process any feedback you have about PlayFab services.

[Forums, Support and Knowledge Base](https://community.playfab.com/index.html)
