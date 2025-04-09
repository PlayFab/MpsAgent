// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System.Collections.Generic;

    public class AssetDetail
    {
        /// <summary>
        /// The path at which the asset should be mounted for each game container.
        /// </summary>
        public string MountPath { get; set; }

        /// <summary>
        /// A prioritized list of shared access signature tokens one per region. The first token is for the storage account in the local region.
        /// The next few tokens should be for the nearby regions in case some accounts are unavailable.
        /// </summary>
        public IList<string> SasTokens { get; set; }

        /// <summary>
        /// For developer testing, specify a file path accessible locally.
        /// </summary>
        public string LocalFilePath { get; set; }

        public AssetDetail ToRedacted()
        {
            AssetDetail clone = (AssetDetail)this.MemberwiseClone();
            clone.SasTokens = null;
            return clone;
        }
    }
}
