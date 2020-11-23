// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.LocalMultiplayerAgent.Config
{
    using System;
    using System.Collections.Generic;
    using AgentInterfaces;

    public class ContainerStartParameters
    {
        public ResourceLimits ResourceLimits { get; set; }

        public ContainerImageDetails ImageDetails { get; set; }

        public string StartGameCommand { get; set; }
    }
}
