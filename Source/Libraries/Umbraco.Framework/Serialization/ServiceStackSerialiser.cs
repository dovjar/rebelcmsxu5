namespace Umbraco.Framework.Serialization
{
    using System;
    using System.IO;
    using ServiceStack.Text;

    public class ServiceStackSerialiser : ISerializer
    {
        public ServiceStackSerialiser()
        {
            JsConfig.DateHandler = JsonDateHandler.ISO8601;
            JsConfig.ExcludeTypeInfo = false;
            JsConfig.IncludeNullValues = true;
            JsConfig.ThrowOnDeserializationError = true;
            JsConfig<HiveId>.DeSerializeFn = s => HiveId.Parse(s);
        }

        public object FromStream(Stream input, Type outputType)
        {
            return JsonSerializer.DeserializeFromStream(outputType, input);
        }

        public IStreamedResult ToStream(object input)
        {
            var ms = new MemoryStream();
            JsonSerializer.SerializeToStream(input, ms);
            return new StreamedResult(ms, true);
        }
    }
}