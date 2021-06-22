namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    public class VmNetworkConfiguration
    {
        public string PublicIpv4Address { get; set; }

        public string Fqdn { get; set; }

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