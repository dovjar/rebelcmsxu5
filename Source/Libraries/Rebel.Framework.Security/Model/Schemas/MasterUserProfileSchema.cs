using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeTypes;
using Rebel.Framework.Persistence.Model.Constants.Schemas;

namespace Rebel.Framework.Security.Model.Schemas
{
    public class MasterUserProfileSchema : ProfileSchema
    {
        public const string SchemaAlias = "system-master-user-profile";
        public const string PersistLoginAlias = "persistLogin";
        public const string SessionTimeoutAlias = "persistedLoginDuration";
        public const string StartContentHiveIdAlias = "startContentHiveId";
        public const string StartMediaHiveIdAlias = "startMediaHiveId";
        public const string ApplicationsAlias = "applications";

        public MasterUserProfileSchema()
        {
            this.Setup(SchemaAlias, "Master User Profile");

            Id = FixedHiveIds.MasterUserProfileSchema;
            SchemaType = FixedSchemaTypes.MasterUserProfile;

            CreatedAttributeDefinitions();
        }

        protected override AttributeGroup DetailsGroup
        {
            get { return FixedGroupDefinitions.UserDetails; }
        }

        protected override void CreatedAttributeDefinitions()
        {
            base.CreatedAttributeDefinitions();

            var inBuiltContentPickerType = AttributeTypeRegistry.Current.GetAttributeType(ContentPickerAttributeType.AliasValue);
            var inBuiltMediaPickerType = AttributeTypeRegistry.Current.GetAttributeType(MediaPickerAttributeType.AliasValue);
            var inBuiltApplicationsListPickerType = AttributeTypeRegistry.Current.GetAttributeType(ApplicationsListPickerAttributeType.AliasValue);
            var inBuildIntegerType = AttributeTypeRegistry.Current.GetAttributeType(IntegerAttributeType.AliasValue);

            this.AttributeDefinitions.Add(new AttributeDefinition(SessionTimeoutAlias, "Persisted login duration")
            {
                AttributeType = inBuildIntegerType,
                AttributeGroup = DetailsGroup,
                Ordinal = 2
            });

            this.AttributeDefinitions.Add(new AttributeDefinition(StartContentHiveIdAlias, "Start node in Content")
            {
                AttributeType = inBuiltContentPickerType,
                AttributeGroup = DetailsGroup,
                Ordinal = 3
            });

            this.AttributeDefinitions.Add(new AttributeDefinition(StartMediaHiveIdAlias, "Start node in Media")
            {
                AttributeType = inBuiltMediaPickerType,
                AttributeGroup = DetailsGroup,
                Ordinal = 4
            });

            this.AttributeDefinitions.Add(new AttributeDefinition(ApplicationsAlias, "Sections")
            {
                AttributeType = inBuiltApplicationsListPickerType,
                AttributeGroup = DetailsGroup,
                Ordinal = 5
            });
        }
        
    }
}
