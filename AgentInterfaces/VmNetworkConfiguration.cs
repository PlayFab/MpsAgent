namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using ProtoBuf;

    [ProtoContract]
    public class VmNetworkConfiguration
    {
        [ProtoMember(1)]
        public string VmName { get; set; }

        /// <summary>
        /// The public Ipv4 address that can be used to connect to the VM.
        /// </summary>
        [ProtoMember(2)]
        public string PublicIpv4Address { get; set; }

        /// <summary>
        /// The fully qualified domain name for the VM. Useful for scenarios involving IPv6 (where the Ipv4 address cannot be used).
        /// </summary>
        [ProtoMember(3)]
        public string Fqdn { get; set; }

        /// <summary>
        /// A list of endpoints on the VM which can be assigned to the game servers so that the game clients can connect to them.
        /// </summary>
        [ProtoMember(4)]
        public Endpoint[] Endpoints { get; set; }
    }

    [ProtoContract]
    public class Endpoint
    {
        /// <summary>
        /// The publicly accessible port exposed on the software load balancer.
        /// </summary>
        [ProtoMember(1)]
        public int FrontEndPort { get; set; }

        /// <summary>
        /// The port on the Node that the FrontEndPort is mapped to.
        /// </summary>
        [ProtoMember(2)]
        public int BackEndPort { get; set; }

        /// <summary>
        /// The protocol for the network traffic.
        /// </summary>
        [ProtoMember(3)]
        public string Protocol { get; set; }
    }
}