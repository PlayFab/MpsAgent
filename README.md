# MpsAgent

## Overview

This repository contains source code for the following projects:

- LocalMultiplayerAgent

An executable that mimics PlayFab Multiplayer Servers (MPS) operations to aid in local debugging. Follow the [Local Debug instructions for usage](https://docs.microsoft.com/en-us/gaming/playfab/features/multiplayer/servers/locally-debugging-game-servers-and-integration-with-playfab). If you want to develop for Linux Containers on Windows, check out [this document](lcow.md).

> This repository replaces and deprecates the project in [this GitHub repo](https://github.com/PlayFab/LocalMultiplayerAgent). The executable produced by that project was called `MockAgent`. This has been renamed to `LocalMultiplayerAgent` for consistency. 

- AgentInterfaces and VmAgent.Core

These two helper libraries are used by LocalMultiplayerAgent and the production [Azure PlayFab Multiplayer Servers service](https://docs.microsoft.com/en-us/gaming/playfab/features/multiplayer/servers/).

## Building

You need to have [.NET Core](https://dotnet.microsoft.com/download) installed and then just run ...

```bash
git clone https://github.com/PlayFab/MpsAgent.git
cd LocalMultiplayerAgent 
dotnet build
```

## Downloading

You can download the latest release of LocalMultiplayerAgent by navigating to the Releases section [here](https://github.com/PlayFab/MpsAgent/releases).

## Contributing

We are more than happy to accept external contributions! If you want to propose a small code change to LocalMultiplayerAgent, feel free to open a Pull Request directly. If you plan to do a bigger change to LocalMultiplayerAgent, it would be better if you open an issue describing your proposed design in order to get feedback from project maintainers.

For AgentInterfaces and VmAgent.Core, we'd recommend to open an issue describing your proposal/idea. Since these projects are used by the service currently in production, we'd need to do additional validation and testing before accepting external contributions.

Furthermore, the YAML files (AgentInterfaces.yml, LocalMultiplayerAgent.yml, VmAgent.Core.yml) should not be altered since they are used by our internal Azure DevOps pipelines.

## Contact Us

We love to hear from our developer community!
Do you have ideas on how we can make our products and services better?

Our Developer Success Team can assist with answering any questions as well as process any feedback you have about PlayFab services.

[Forums, Support and Knowledge Base](https://community.playfab.com/index.html)