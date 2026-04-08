#!/bin/bash

# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# This script sets up your Linux machine to run the PlayFab Multiplayer Server LocalMultiplayerAgent
# Prerequisites: Docker must be installed and running (only needed for container mode)

set -euo pipefail

AGENT_PORT=${1:-56001}

echo "Setting up LocalMultiplayerAgent for Linux..."

# Check if Docker is installed and running (only required for container mode)
if command -v docker &> /dev/null; then
    if docker info &> /dev/null 2>&1; then
        # Create the playfab Docker network if it doesn't exist
        EXISTING_NETWORK=$(docker network ls --filter name=^playfab$ --format "{{.Name}}")
        if [ -z "$EXISTING_NETWORK" ]; then
            echo "Creating 'playfab' Docker network..."
            docker network create playfab --driver bridge
            echo "Docker network 'playfab' created successfully."
        else
            echo "Docker network 'playfab' already exists."
        fi
    else
        echo "Warning: Docker is not running. Docker is required for container mode."
        echo "If you plan to use process mode only, you can ignore this warning."
    fi
else
    echo "Warning: Docker is not installed. Docker is required for container mode."
    echo "If you plan to use process mode only, you can ignore this warning."
fi

echo ""
echo "Setup complete! You can now run LocalMultiplayerAgent."
echo "Agent will listen on port $AGENT_PORT."
echo ""
echo "Make sure to update MultiplayerSettings.json with your game server details."
echo "You can use MultiplayerSettingsLinuxContainersOnLinuxSample.json (container mode)"
echo "or MultiplayerSettingsLinuxProcessOnLinuxSample.json (process mode) as a reference."
