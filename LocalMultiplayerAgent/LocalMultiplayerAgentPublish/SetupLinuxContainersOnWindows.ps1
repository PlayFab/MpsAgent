# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# This file is to support Linux Containers development on a Windows host using Docker for Windows
# Create a custom docker network used by containers to talk to the Agent.
docker network create playfab --driver bridge
