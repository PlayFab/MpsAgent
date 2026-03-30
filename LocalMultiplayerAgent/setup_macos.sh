#!/bin/bash

# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# This script sets up your Mac to run the PlayFab Multiplayer Server LocalMultiplayerAgent
# Prerequisites: Docker Desktop for Mac must be installed and running

set -euo pipefail

AGENT_PORT=${1:-56001}

echo "Setting up LocalMultiplayerAgent for MacOS..."

# Check if Docker is installed and running
if ! command -v docker &> /dev/null; then
    echo "Error: Docker is not installed. Please install Docker Desktop for Mac from https://www.docker.com/products/docker-desktop/"
    exit 1
fi

if ! docker info &> /dev/null 2>&1; then
    echo "Error: Docker is not running. Please start Docker Desktop."
    exit 1
fi

# Create the playfab Docker network if it doesn't exist
EXISTING_NETWORK=$(docker network ls --filter name=^playfab$ --format "{{.Name}}")
if [ -z "$EXISTING_NETWORK" ]; then
    echo "Creating 'playfab' Docker network..."
    docker network create playfab --driver bridge
    echo "Docker network 'playfab' created successfully."
else
    echo "Docker network 'playfab' already exists."
fi

echo ""
echo "Setup complete! You can now run LocalMultiplayerAgent."
echo "Agent will listen on port $AGENT_PORT."
echo ""
echo "Make sure to update MultiplayerSettings.json with your game server container image details."
echo "You can use MultiplayerSettingsLinuxContainersOnMacOSSample.json as a reference."
