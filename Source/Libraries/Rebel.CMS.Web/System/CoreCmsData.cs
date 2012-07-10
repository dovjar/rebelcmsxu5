using System;
using System.Collections.Generic;
using System.Linq;
using Rebel.Framework;
using Rebel.Framework.Context;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Associations;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeTypes;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Persistence.Model.Constants.SerializationTypes;
using Rebel.Framework.Security;
using Rebel.Framework.Security.Model.Entities;

namespace Rebel.Cms.Web.System
{
    /// <summary>
    /// A class defining the required core data that is needed for the CMS to operate
    /// </summary>
    internal static class CoreCmsData
    {
        internal static IEnumerable<UserGroup> RequiredCoreUserGroups(
            IEnumerable<Lazy<Permission, PermissionMetadata>> permissions)
        {
            var admin = new UserGroup()
            {
                Name = "Administrator"
            };

            var adminMetaDataumList = permissions.Where(x => (x.Metadata.UserType & UserType.User) == UserType.User)
                .Select(p => new RelationMetaDatum(p.Metadata.Id.ToString(), PermissionStatus.Allow.ToString()));
            admin.RelationProxies.EnlistChildById(FixedHiveIds.SystemRoot, FixedRelationTypes.PermissionRelationType, 0, adminMetaDataumList.ToArray());

            return new[] { admin };
        }

        //internal static IEnumerable<UserGroup> RequiredCoreMemberGroups(
        //    IEnumerable<Lazy<Permission, PermissionMetadata>> permissions)
        //{
        //    var anonymous = new UserGroup()
        //    {
        //        Name = "Anonymous"
        //    };

        //    var anonymousMetaDataumList = permissions.Where(x => (x.Metadata.UserType & UserType.Member) == UserType.Member)
        //        .Select(p => new RelationMetaDatum(p.Metadata.Id.ToString(), (p.Metadata.Id.ToString().Equals(FixedPermissionIds.View, StringComparison.InvariantCultureIgnoreCase)) ? PermissionStatus.Allow.ToString() : PermissionStatus.Deny.ToString()));
        //    anonymous.RelationProxies.EnlistChildById(FixedHiveIds.SystemRoot, FixedRelationTypes.PermissionRelationType, 0, anonymousMetaDataumList.ToArray());

        //    return new[] { anonymous };
        //}

        //internal static IEnumerable<User> RequiredCoreUsers()
        //{
        //    var admin = new User()
        //        {
        //            Name = "Administrator",
        //            Username = "admin",
        //            Password = "test",
        //            Email = "admin@domain.com",
        //            StartContentHiveId = FixedHiveIds.ContentVirtualRoot,
        //            StartMediaHiveId = FixedHiveIds.MediaVirtualRoot,
        //            Applications = new List<string>(new[] {"content", "media", "settings", "developer", "users"}),
        //            IsApproved = true,
        //            SessionTimeout = 60
        //        };
            
        //    return new[] {admin };
        //}

        /// <summary>
        /// Returns in-built attribute type definitions with the correct render type provider and 
        /// pre-values setup for the CMS
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<AttributeType> RequiredCoreSystemAttributeTypes()
        {
            return new[]
                {
                    AttributeTypeRegistry.Current.GetAttributeType(StringAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(TextAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(IntegerAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(DateTimeAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(BoolAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(ReadOnlyAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(ContentPickerAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(MediaPickerAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(ApplicationsListPickerAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(NodeNameAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(SelectedTemplateAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(UserGroupUsersAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(FileUploadAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(DictionaryItemTranslationsAttributeType.AliasValue)
                };
        }

        /// <summary>
        /// returns a list of attribute types to be installed by default that the user can use
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<AttributeType> RequiredCoreUserAttributeTypes()
        {
            return new []
                {
                    AttributeTypeRegistry.Current.GetAttributeType("richTextEditor"),
                    AttributeTypeRegistry.Current.GetAttributeType("singleLineTextBox"),
                    AttributeTypeRegistry.Current.GetAttributeType("multiLineTextBox"),
                    AttributeTypeRegistry.Current.GetAttributeType("multiLineTextBoxWithControls"),
                    AttributeTypeRegistry.Current.GetAttributeType("colorSwatchPicker"),
                    AttributeTypeRegistry.Current.GetAttributeType("tags"),
                    AttributeTypeRegistry.Current.GetAttributeType("contentPicker"),
                    AttributeTypeRegistry.Current.GetAttributeType("mediaPicker"),
                    AttributeTypeRegistry.Current.GetAttributeType("integer"),
                    AttributeTypeRegistry.Current.GetAttributeType("decimal"),
                    AttributeTypeRegistry.Current.GetAttributeType("uploader"),
                    AttributeTypeRegistry.Current.GetAttributeType("trueFalse"),
                    AttributeTypeRegistry.Current.GetAttributeType("dropdownList"),
                    AttributeTypeRegistry.Current.GetAttributeType("listBox"),
                    AttributeTypeRegistry.Current.GetAttributeType("checkboxList"),
                    AttributeTypeRegistry.Current.GetAttributeType("radioButtonList"),
                    AttributeTypeRegistry.Current.GetAttributeType("dateTimePicker"),
                    AttributeTypeRegistry.Current.GetAttributeType("slider"),
                    AttributeTypeRegistry.Current.GetAttributeType("label")
                };
        }

        /// <summary>
        /// All of the required/root schemas
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<EntitySchema> RequiredCoreSchemas()
        {
            //need to update the media schemas to allow folders and images under folder
            var image = FixedSchemas.MediaImageSchema;
            var folder = FixedSchemas.MediaFolderSchema;
            folder.SetXmlConfigProperty("allowed-templates", new[]
                {
                    image.Id.ToString(),
                    folder.Id.ToString()
                });

            return new EntitySchema[]
                {
                    // Create in-built schemas
                    Rebel.Framework.Security.Model.FixedSchemas.UserGroup,
                    FixedSchemas.ContentRootSchema,
                    FixedSchemas.MediaRootSchema,
                    folder,
                    image,
                    FixedSchemas.HostnameSchema,
                    FixedSchemas.DictionaryRootSchema,
                    FixedSchemas.DictionaryItemSchema,
                    Rebel.Framework.Security.Model.FixedSchemas.MembershipUserSchema,
                    Rebel.Framework.Security.Model.FixedSchemas.UserProfileSchema,
                    Rebel.Framework.Security.Model.FixedSchemas.MemberProfileSchema
                };
        }      

        /// <summary>
        /// Returns all of the required entities for the CMS to operate
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<TypedEntity> RequiredCoreRootNodes()
        {
            return new TypedEntity[]
                {
                    // Create root nodes
                    new SystemRoot(), 
                    FixedEntities.ContentVirtualRoot,
                    FixedEntities.ContentRecycleBin,
                    FixedEntities.MediaVirtualRoot,
                    FixedEntities.MediaRecycleBin,
                    FixedEntities.DictionaryVirtualRoot,
                    Framework.Security.Model.FixedEntities.UserVirtualRoot,
                    Framework.Security.Model.FixedEntities.MemberVirtualRoot,
                    Framework.Security.Model.FixedEntities.MemberProfileVirtualRoot,
                    Framework.Security.Model.FixedEntities.UserProfileVirtualRoot,
                    Framework.Security.Model.FixedEntities.UserGroupVirtualRoot,
                    Framework.Security.Model.FixedEntities.MemberGroupVirtualRoot
                };
        }

    }
}