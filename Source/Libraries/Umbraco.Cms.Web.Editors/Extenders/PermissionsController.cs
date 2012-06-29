using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.ViewEngines;
using Umbraco.Cms.Web.Security;
using Umbraco.Framework;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Security;
using Umbraco.Framework.Security.Model.Entities;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;
using FixedHiveIds = Umbraco.Framework.Security.Model.FixedHiveIds;

namespace Umbraco.Cms.Web.Editors.Extenders
{

    public class PermissionsController : ContentControllerExtenderBase
    {
        public PermissionsController(IBackOfficeRequestContext backOfficeRequestContext)
            : base(backOfficeRequestContext)
        { }

        /// <summary>
        /// Permissionses the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [HttpGet]
        [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Permissions })] 
        public virtual ActionResult Permissions(HiveId id)
        {
            var model = new PermissionsModel { Id = id };

            // Get all permissions
            var permissions = BackOfficeRequestContext.RegisteredComponents.Permissions
                .Where(x => x.Metadata.Type == FixedPermissionTypes.EntityAction && (x.Metadata.UserType & UserType.User) == UserType.User)
                .Select(x => x.Metadata)
                .OrderBy(x => x.Name)
                .ToList();

            // Create user groups permission model
            using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<ISecurityStore>())
            {
                var userGroupPermissionsModels = uow.Repositories.GetChildren<UserGroup>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserGroupVirtualRoot)
                    .Where(x => x.Name != "Administrator")
                    .OrderBy(x => x.Name)
                    .Select(x => BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<UserGroupPermissionsModel>(x))
                    .ToList();

                foreach (var userGroupPermissionsModel in userGroupPermissionsModels)
                {
                    var permissionStatusModels = permissions.Select(x => BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<PermissionStatusModel>(x)).ToList();
                    
                    foreach (var permissionStatusModel in permissionStatusModels)
                    {
                        // Set status
                        permissionStatusModel.Status = BackOfficeRequestContext.Application.Security.Permissions.GetExplicitPermission(permissionStatusModel.PermissionId, new[] { userGroupPermissionsModel.UserGroupId }, id).Status;

                        // Set inherited status
                        permissionStatusModel.InheritedStatus = BackOfficeRequestContext.Application.Security.Permissions.GetInheritedPermission(permissionStatusModel.PermissionId, new[] { userGroupPermissionsModel.UserGroupId }, id).Status;
                    }

                    userGroupPermissionsModel.Permissions = permissionStatusModels;
                }

                model.UserGroupPermissions = userGroupPermissionsModels;
            }

            return View(model);
        }

        /// <summary>
        /// Permissionses the form.
        /// </summary>
        /// <returns></returns>
        [ActionName("Permissions")]
        [HttpPost]
        //[UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Permissions })] 
        public virtual JsonResult PermissionsForm(PermissionsModel model)
        {
            using (var uow = Hive.Create<IContentStore>())
            {
                var entity = uow.Repositories.Get<TypedEntity>(model.Id);
                if (entity == null)
                    throw new NullReferenceException("Could not find entity with id " + model.Id);

                foreach (var userGroupPermissionsModel in model.UserGroupPermissions)
                {
                    var exists = uow.Repositories.Exists<UserGroup>(userGroupPermissionsModel.UserGroupId);
                    if (!exists)
                        throw new NullReferenceException("Could not find entity with id " + userGroupPermissionsModel.UserGroupId);

                    // Populate metadatum collection based on selection
                    var metaDataumList = new List<RelationMetaDatum>();
                    foreach (var permissionModel in userGroupPermissionsModel.Permissions)
                    {
                        var permission = BackOfficeRequestContext.RegisteredComponents.Permissions.SingleOrDefault(x => x.Metadata.Id == permissionModel.PermissionId);
                        if (permission == null)
                            throw new NullReferenceException("Could not find permission with id " + permissionModel.PermissionId);

                        metaDataumList.Add(BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<RelationMetaDatum>(permissionModel));
                    }

                    // Change permissions relation
                    uow.Repositories.ChangeOrCreateRelationMetadata(userGroupPermissionsModel.UserGroupId, entity.Id, FixedRelationTypes.PermissionRelationType, metaDataumList.ToArray());
                }

                uow.Complete();

                var successMsg = "Permissions.Success.Message".Localize(this, new
                    {
                        Name = entity.GetAttributeValue(NodeNameAttributeDefinition.AliasValue, "Name")
                    });

                Notifications.Add(new NotificationMessage(
                                      successMsg,
                                      "Permissions.Title".Localize(this), NotificationType.Success));

                return new CustomJsonResult(new
                    {
                        success = true,
                        notifications = Notifications,
                        msg = successMsg
                    }.ToJsonString);
            }
        }
    }
}