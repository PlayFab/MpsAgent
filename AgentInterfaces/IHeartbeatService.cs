// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System.ServiceModel;
    using System.Threading.Tasks;
    using ProtoBuf.Grpc;

    [ServiceContract]
    public interface IHeartbeatService
    {
        [OperationContract]
        ValueTask<HeartbeatResponse> SendHeartbeat(HeartbeatRequest request, CallContext context = default);
        // documentation for CallContext https://protobuf-net.github.io/protobuf-net.Grpc/configuration
    }
}
