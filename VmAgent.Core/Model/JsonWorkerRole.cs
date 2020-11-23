// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Small helper class so JSON.Net
    /// deserializes key/value pairs with
    /// our expected name/value properties
    /// </summary>
    public class JsonConfigSetting
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// Models jsonWorkerRole object in legacy ServiceDefinition.json
    /// </summary>
    public class JsonWorkerRole
    {
        // This is our source of truth
        private Dictionary<string, string> _configurationDict = new Dictionary<string, string>();

        // We need to use a List so JSON.Net deserializes in the expected format.
        // ObjectCreationHandling is required to ensure that setter is always called during deserialization.
        [JsonProperty("configurationSettings", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<JsonConfigSetting> ConfigurationSettings
        {
            get => _configurationDict.Select(item => new JsonConfigSetting { Name = item.Key, Value = item.Value }).ToList();

            set => _configurationDict = value.ToDictionary(x => x.Name, x => x.Value);
        }

        // Provide properties to easily access the required settings, but we don't want to serialize them
        [JsonIgnore]
        public bool NoGSMS
        {
            get => Convert.ToBoolean(_configurationDict["no_gsms"]);
            set => _configurationDict["no_gsms"] = value.ToString().ToUpperInvariant();
        }

        [JsonIgnore]
        public string TitleId
        {
            get => _configurationDict["titleId"];
            set => _configurationDict["titleId"] = value;
        }

        [JsonIgnore]
        public string ClusterId
        {
            get => _configurationDict["clusterId"];
            set => _configurationDict["clusterId"] = value;
        }

        [JsonIgnore]
        public string GsmsBaseUrl
        {
            get => _configurationDict["gsmsBaseUrl"];
            set => _configurationDict["gsmsBaseUrl"] = value;
        }

        [JsonIgnore]
        public string GsiId
        {
            get => _configurationDict["gsiId"];
            set => _configurationDict["gsiId"] = value;
        }

        [JsonIgnore]
        public string GsiSetId
        {
            get => _configurationDict["gsiSetId"];
            set => _configurationDict["gsiSetId"] = value;
        }

        [JsonIgnore]
        public string SessionHostId
        {
            get => _configurationDict["sessionHostId"];
            set => _configurationDict["sessionHostId"] = value;
        }

        [JsonIgnore]
        public string InstanceId
        {
            get => _configurationDict["instanceId"];
            set => _configurationDict["instanceId"] = value;
        }

        [JsonIgnore]
        public string ExeFolderPath
        {
            get => _configurationDict["exeFolderPath"];
            set => _configurationDict["exeFolderPath"] = value;
        }

        [JsonIgnore]
        public string XblIpsecCertificateThumbprint
        {
            get => _configurationDict["xblIpsecCertificateThumbprint"];
            set => _configurationDict["xblIpsecCertificateThumbprint"] = value;
        }

        [JsonIgnore]
        public string XblGameServerCertificateThumbprint
        {
            get => _configurationDict["xblGameServerCertificateThumbprint"];
            set => _configurationDict["xblGameServerCertificateThumbprint"] = value;
        }

        [JsonIgnore]
        public string TenantName
        {
            get => _configurationDict["tenantName"];
            set => _configurationDict["tenantName"] = value;
        }

        [JsonIgnore]
        public int TenantCount
        {
            get => Convert.ToInt32(_configurationDict["tenantCount"]);
            set => _configurationDict["tenantCount"] = value.ToString();
        }

        [JsonIgnore]
        public string Location
        {
            get => _configurationDict["location"];
            set => _configurationDict["location"] = value;
        }

        [JsonIgnore]
        public string Datacenter
        {
            get => _configurationDict["datacenter"];
            set => _configurationDict["datacenter"] = value;
        }

        [JsonIgnore]
        public string XassBaseUrl
        {
            get => _configurationDict["xassBaseUrl"];
            set => _configurationDict["xassBaseUrl"] = value;
        }

        [JsonIgnore]
        public string XastBaseUrl
        {
            get => _configurationDict["xastBaseUrl"];
            set => _configurationDict["xastBaseUrl"] = value;
        }

        [JsonIgnore]
        public string XstsBaseUrl
        {
            get => _configurationDict["xstsBaseUrl"];
            set => _configurationDict["xstsBaseUrl"] = value;
        }

        /// <summary>
        /// Allows us to set any generic configuration value.
        /// Useful for setting thumbprints for game-specific certs
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        public void SetConfigValue(string key, string value)
        {
            _configurationDict[key] = value;
        }
    }
}
