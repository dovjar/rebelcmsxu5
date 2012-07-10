using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Rebel.Framework.Security
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PermissionStatus
    {
        Allow,
        Deny,
        Inherit
    }
}
