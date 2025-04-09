// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System;

    public class CertificateDetail
    {
        /// <summary>
        /// Identifier for the certificate. When we save it
        /// to the VM, the filename will be [Name].pfx
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The contents of the PFX file for this certificate
        /// </summary>
        /// <remarks>
        /// Only one of PfxContents or PemContents should be filled in for any particular cert
        /// </remarks>
        public byte[] PfxContents { get; set; }

        /// <summary>
        /// The contents of the PEM file for this certificate
        /// </summary>
        /// <remarks>
        /// Only one of PfxContents or PemContents should be filled in for any particular cert
        /// </remarks>
        public string PemContents { get; set; }

        /// <summary>
        /// The thumprint of this certificate
        /// </summary>
        public string Thumbprint { get; set; }

        /// <summary>
        /// The version of this certificate
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Expiration date of this certificate
        /// </summary>
        public string ExpirationDate { get; set; }

        /// <summary>
        /// Expiration date of the certificate.
        /// </summary>
        public DateTime ExpirationTimeUtc { get; set; }

        public CertificateDetail ToRedacted()
        {
            CertificateDetail clone = (CertificateDetail)this.MemberwiseClone();
            clone.PfxContents = null;
            clone.PemContents = null;

            return clone;
        }
    }
}
