// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using ProtoBuf;

    [JsonConverter(typeof(StringEnumConverter))]
    [ProtoContract]
    public enum Operation
    {
        [ProtoEnum]
        Invalid,
        [ProtoEnum]
        Continue,
        [ProtoEnum]
        GetManifest,
        [ProtoEnum]
        Quarantine,
        [ProtoEnum]
        Active,
        [ProtoEnum]
        Terminate
    }
}
