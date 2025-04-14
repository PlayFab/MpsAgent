// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System;

    public class SecretDetail
    {
        /// <summary>
        /// Secret identifier
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Secret Value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The version of this secret in the internal Key Vault
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Expiration date of this secret
        /// </summary>
        public string ExpirationDate { get; set; }

        public SecretDetail ToRedacted()
        {
            SecretDetail clone = (SecretDetail)this.MemberwiseClone();
            clone.Value = null;

            return clone;
        }
    }
}
