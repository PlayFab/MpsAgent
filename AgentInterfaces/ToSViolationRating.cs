namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ToSViolationRating
    {
        Low,
        Medium,
        High
    }
}
