using System;
using System.Globalization;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Framework;

namespace Umbraco.Tests.CoreAndFramework
{
    [TestFixture]
    public class HiveIdJsonConverterTests
    {
        public class CustomObject
        {
            public string Name { get; set; }
            
            [JsonConverter(typeof(HiveIdJsonConverter))]
            public HiveId Id { get; set; }
        }

        [TestCase]
        public void Serialized_To_Json_Object()
        {
            var obj = new CustomObject()
                {
                    Name = "Testing",
                    Id = new HiveId("content", "my-provider", new HiveIdValue(Guid.NewGuid()))
                };
            var hiveId = obj.Id;

            var result = JObject.FromObject(obj);
            Assert.IsNotNull(result["Id"]);
            var json = result["Id"];

            Assert.AreEqual(json["htmlId"].ToString(), hiveId.GetHtmlId());
            Assert.AreEqual(json["rawValue"].ToString(), hiveId.ToString());
            Assert.AreEqual(json["value"].ToString(), hiveId.Value.Value.ToString());
            Assert.AreEqual(json["valueType"].ToString(), ((int)hiveId.Value.Type).ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(json["provider"].ToString(), hiveId.ProviderId);
            Assert.AreEqual(json["scheme"].ToString(), hiveId.ProviderGroupRoot.ToString());
        }
    }
}