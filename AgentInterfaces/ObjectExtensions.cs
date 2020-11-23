// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System;
    using Newtonsoft.Json;

    public static class ObjectExtensions
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        public static string ToJsonString(this object value)
        {
            return JsonConvert.SerializeObject(value, JsonSerializerSettings);
        }

        public static T FromJsonString<T>(this string jsonValue)
        {
            return JsonConvert.DeserializeObject<T>(jsonValue, JsonSerializerSettings);
        }

        public static object FromJsonString(this string jsonValue, Type type)
        {
            return JsonConvert.DeserializeObject(jsonValue, type, JsonSerializerSettings);
        }
    }
}
