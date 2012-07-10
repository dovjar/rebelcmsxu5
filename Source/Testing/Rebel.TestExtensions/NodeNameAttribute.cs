using Rebel.Framework.Persistence.Model.Attribution;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;

namespace Rebel.Tests.Extensions
{

    /// <summary>
    /// Represents an attribute storing the name for the entity
    /// </summary>
    public class NodeNameAttribute : TypedAttribute
    {
        public NodeNameAttribute()
            : base(new NodeNameAttributeDefinition())
        {}

        public NodeNameAttribute(string nodeName, AttributeGroup group)
            : base(new NodeNameAttributeDefinition(group))
        {
            //the node name by default needs to go into the 'Name' portion of the values colleciton
            Values["Name"] = nodeName;
            Values["UrlName"] = "";
        }
    }
}