// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResourceRetrievalResult
    {
        Unknown,
        Success,
        AuthenticationFailure,
        ResourceNotAvailable,
        ResourceNotFound,
        TooManyRequests,
        Other
    }
}
