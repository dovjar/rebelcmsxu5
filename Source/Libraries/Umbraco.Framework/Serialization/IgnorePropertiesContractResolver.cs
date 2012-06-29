using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Umbraco.Framework.Serialization
{
    public class IgnorePropertiesContractResolver : DefaultContractResolver
    {
        public IgnorePropertiesContractResolver()
        {
        }

        public IgnorePropertiesContractResolver(bool shareCache) : base(shareCache)
        {
        }

        /// <summary>
        /// Overrides standard CreateProperties method in order to 
        /// remove the properties that we don't want serialized.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="memberSerialization"></param>
        /// <returns></returns>
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);

            if (type.Name.Equals("TypedEntity"))
            {
                properties.RemoveAll(p => p.PropertyName.Equals("RelationProxies"));
            }

            if(type.Name.Equals("EntitySchema"))
            {
                properties.RemoveAll(p => p.PropertyName.Equals("AttributeGroups"));
            }

            if (type.Name.Equals("TypedAttribute") || (type.BaseType != null && type.BaseType.Name.Equals("TypedAttribute")))
            {
                properties.RemoveAll(p => p.PropertyName.Equals("DynamicValue"));                
            }

            return properties;
        }
    }
}
