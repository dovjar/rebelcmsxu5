using System;
using System.IO;

namespace Umbraco.Framework.Serialization
{
    using System.Text;

    public static class SerializationExtensions
    {
        public static T FromJson<T>(this AbstractSerializationService service, string json, string intent = null)
        {
            if (json.IsNullOrWhiteSpace()) return default(T);
            return (T)service.FromJson(json, typeof(T), intent);
        }

        public static T FromJson<T>(this ISerializer serializer, string json, string intent = null)
        {
            if (json.IsNullOrWhiteSpace()) return default(T);
            return (T)serializer.FromJson(json, typeof(T));
        }

        public static object FromJson(this ISerializer serializer, string json, Type outputType)
        {
            if (json.IsNullOrWhiteSpace()) return outputType.GetDefaultValue();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            return serializer.FromStream(stream, outputType);
        }

        public static object FromJson(this AbstractSerializationService service, string json, Type outputType, string intent = null)
        {
            if (json.IsNullOrWhiteSpace()) return outputType.GetDefaultValue();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            return service.FromStream(stream, outputType, intent);
        }

        public static string ToJson(this AbstractSerializationService service, object input, string intent = null)
        {
            return service.ToStream(input, intent).ResultStream.ToJsonString();
        }
    }

    public class SerializationService : AbstractSerializationService
    {
        private readonly ISerializer _serializer;

        public SerializationService(ISerializer serializer)
        {
            _serializer = serializer;
        }

        #region Overrides of AbstractSerializationService

        /// <summary>
        ///   Finds an <see cref="IFormatter" /> with a matching <paramref name="intent" /> , and deserializes the <see cref="Stream" /> <paramref
        ///    name="input" /> to an object graph.
        /// </summary>
        /// <param name="input"> </param>
        /// <param name="outputType"> </param>
        /// <param name="intent"> </param>
        /// <returns> </returns>
        public override object FromStream(Stream input, Type outputType, string intent = null)
        {
            if (input.CanSeek && input.Position > 0) input.Seek(0, SeekOrigin.Begin);
            return _serializer.FromStream(input, outputType);
        }

        /// <summary>
        ///   Finds an <see cref="IFormatter" /> with a matching <paramref name="intent" /> , and serializes the <paramref
        ///    name="input" /> object graph to an <see cref="IStreamedResult" /> .
        /// </summary>
        /// <param name="input"> </param>
        /// <param name="intent"> </param>
        /// <returns> </returns>
        public override IStreamedResult ToStream(object input, string intent = null)
        {
            return _serializer.ToStream(input);
        }

        #endregion
    }
}
