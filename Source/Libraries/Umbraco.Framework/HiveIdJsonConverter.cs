using System;
using Newtonsoft.Json;

namespace Umbraco.Framework
{
    /// <summary>
    /// Used to convert a HiveId to a Json object
    /// </summary>
    /// <remarks>
    /// By default, the HiveId struct is not prefixed to use this converter, this class
    /// exists in case you require a HiveId property of your object to be serialized
    /// to the correct Json format instead of just a string for use in JavaScript.
    /// </remarks>
    public class HiveIdJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                var id = (HiveId)value;
                
                writer.WriteStartObject();
                writer.WritePropertyName("htmlId");
                writer.WriteValue(id.GetHtmlId());

                writer.WritePropertyName("rawValue");
                writer.WriteValue(id.ToString());

                writer.WritePropertyName("value");
                writer.WriteValue(id.Value.Value == null ? "" : id.Value.Value.ToString());

                writer.WritePropertyName("valueType");
                writer.WriteValue(id.Value.Type);

                writer.WritePropertyName("provider");
                writer.WriteValue(id.ProviderId.IsNullOrWhiteSpace() ? "" : id.ProviderId);

                writer.WritePropertyName("scheme");
                writer.WriteValue(id.ProviderGroupRoot == null ? "" : id.ProviderGroupRoot.ToString());

                writer.WriteEndObject();                
            }

        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(HiveId));
        }
    }
}