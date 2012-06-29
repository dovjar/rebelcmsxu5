namespace Umbraco.Framework.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class CustomIgnoreResolver : DefaultContractResolver 
    {
        public CustomIgnoreResolver()
        {}

        public CustomIgnoreResolver(bool shareCache)
            : base(shareCache)
        {}

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            var members = base.GetSerializableMembers(objectType);

            // Json.net doesn't appear to properly honour IgnoreDataMemberAttribute
            // so remove any that have it from the list of serializable members (APN)
            members = new List<MemberInfo>(members.Where(x => !x.GetCustomAttributes<IgnoreDataMemberAttribute>(true).Any()));

            return members;
        }

        protected override JsonContract CreateContract(Type objectType)
        {
            // Json.net handily assumes that if a TypeConverter exists to turn a type into String
            // then that should be used for its Json representation, but that's really not very 
            // helpful for LocalizedString. Here we prevent the base implementation from checking
            // and returning CreateStringContract if the type is LocalizedString (APN)
            if (objectType == typeof(LocalizedString))
                return base.CreateObjectContract(objectType);

            return base.CreateContract(objectType);
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

            if (type.Name.Equals("EntitySchema"))
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