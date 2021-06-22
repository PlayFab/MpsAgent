namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    public class VmNetworkConfiguration
    {
        /// <summary>
        /// The public Ipv4 address that can be used to connect to the VM.
        /// </summary>
        public string PublicIpv4Address { get; set; }

        /// <summary>
        /// The fully qualified domain name for the VM. Useful for scenarios involving IPv6 (where the Ipv4 address cannot be used).
        /// </summary>
        public string Fqdn { get; set; }

        /// <summary>
        /// A list of endpoints on the VM which can be assigned to the game servers so that the game clients can connect to them.
        /// </summary>
        public Endpoint[] Endpoints { get; set; }
    }

    public class Endpoint
    {
        /// <summary>
        /// The publicly accessible port exposed on the software load balancer.
        /// </summary>
        public int? FrontEndPort { get; set; }

        /// <summary>
        /// The port on the Node that the FrontEndPort is mapped to.
        /// </summary>
        public int? BackEndPort { get; set; }

        /// <summary>
        /// The protocol for the network traffic.
        /// </summary>
        public string Protocol { get; set; }
    }
}