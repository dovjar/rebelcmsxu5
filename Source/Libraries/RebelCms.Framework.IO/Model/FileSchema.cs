using RebelCms.Framework.Persistence;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Persistence.Model.Constants.AttributeTypes;

namespace RebelCms.Framework.IO.Model
{
    public class FileSchema : EntitySchema
    {
        public FileSchema()
        {
            SchemaType = FixedSchemaTypes.File;
            AttributeDefinitions.Add(new AttributeDefinition
                                         {
                                             Alias = "name",
                                             Name = "Name",
                                             AttributeType = new StringAttributeType(),
                                             AttributeGroup = FixedGroupDefinitions.GeneralGroup
                                         });

            AttributeDefinitions.Add(new AttributeDefinition
                                         {
                                             Alias = "location",
                                             Name = "Location",
                                             AttributeType = new StringAttributeType(),
                                             AttributeGroup = FixedGroupDefinitions.GeneralGroup
                                         });

            AttributeDefinitions.Add(new AttributeDefinition
                                         {
                                             Alias = "isContainer",
                                             Name = "Is Container",
                                             AttributeType = new IntegerAttributeType(),
                                             AttributeGroup = FixedGroupDefinitions.GeneralGroup
                                         });

            AttributeDefinitions.Add(new AttributeDefinition
                                         {
                                             Alias = "absolutePath",
                                             Name = "Absolute Path",
                                             AttributeType = new StringAttributeType(),
                                             AttributeGroup = FixedGroupDefinitions.GeneralGroup
                                         });
        }
    }
}
