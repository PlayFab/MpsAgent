// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    /// <summary>
    /// Models the configuration json file we write to the containers
    /// for the GSDK to read
    /// </summary>
    public class GsdkConfiguration
    {
        public string HeartbeatEndpoint { get; set; }

        public string SessionHostId { get; set; }

        public string VmId { get; set; }

        public string LogFolder { get; set; }

        public string CertificateFolder { get; set; }

        public string SharedContentFolder { get; set; }

        public Dictionary<string, string> GameCertificates { get; set; }

        public IDictionary<string, string> BuildMetadata { get; set; }

        public IDictionary<string, string> GamePorts { get; set; }

        public string PublicIpV4Address { get; set; }

        public GameServerConnectionInfo GameServerConnectionInfo { get; set; }

        public int ServerInstanceNumber { get; set; }

        public string FullyQualifiedDomainName { get; set; }
    }

    public class GameServerConnectionInfo
    {
        public string PublicIpV4Adress { get; set; }

        public IEnumerable<GamePort> GamePortsConfiguration { get; set; }
    }

    /// <summary>
    /// A class that captures details about a game server port.
    /// </summary>
    public class GamePort
    {
        /// <summary>
        /// The friendly name / identifier for the port, specified by the game developer in the Build configuration.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The port at which the game server should listen on (maps externally to <see cref="ClientConnectionPort"/>).
        /// For process based servers, this is determined by Control Plane, based on the ports available on the VM.
        /// For containers, this is specified by the game developer in the Build configuration.
        /// </summary>
        public int ServerListeningPort { get; set; }

        /// <summary>
        /// The public port to which clients should connect (maps internally to <see cref="ServerListeningPort"/>).
        /// </summary>
        public int ClientConnectionPort { get; set; }
    }
}
