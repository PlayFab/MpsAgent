// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Model
{
    using System;

    /// <summary>
    /// Models the legacy ServiceDefinition.json configuration file,
    /// so games with the old GSDK can connect to this Agent
    /// </summary>
    public class ServiceDefinition
    {
        public CurrentRoleInstance CurrentRoleInstance { get; set; }

        public JsonWorkerRole JsonWorkerRole { get; set; }

        // The Azure cloud service deployment Id. We fake a random Guid. It appears that some games just need a value to be present.
        public string DeploymentId { get; set; }

        // Some games use this before attempting other calls. Just setting it to true always since it isn't relevant for PlayFab.
        public bool IsAvailable { get; set; } = true;
    }
}
