// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    public class ContainerImageDetails
    {
        /// <summary>
        /// FQDN of the container registry.
        /// </summary>
        public string Registry { get; set; }

        /// <summary>
        /// Name of the container image.
        /// </summary>
        public string ImageName { get; set; }

        /// <summary>
        /// Tag of the container image.
        /// </summary>
        public string ImageTag { get; set; }

        /// <summary>
        /// Digest of the container image. Takes precedence over the tag if present.
        /// </summary>
        public string ImageDigest { get; set; }

        /// <summary>
        /// Username to use to authenticate to the container registry. 
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password to use to authenticate to the container registry.
        /// </summary>
        public string Password { get; set; }

    }
}
