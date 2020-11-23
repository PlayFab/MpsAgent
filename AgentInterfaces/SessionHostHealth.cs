// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using ProtoBuf;

    [JsonConverter(typeof(StringEnumConverter))]
    [ProtoContract]
    public enum SessionHostHealth
    {
        [ProtoEnum]
        Healthy,
        [ProtoEnum]
        Unhealthy,
    }
}
