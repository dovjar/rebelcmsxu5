using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Helpers;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Cms.Web.Mvc;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Cms.Web.Mvc.ViewEngines;
using RebelCms.Cms.Web.Security;
using RebelCms.Framework;
using RebelCms.Framework.Localization;
using RebelCms.Framework.Persistence;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Associations;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Persistence.Model.Constants.AttributeDefinitions;
using RebelCms.Framework.Persistence.Model.Constants.Entities;
using RebelCms.Framework.Security;
using RebelCms.Hive;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Cms.Web.Editors.Extenders
{

    public class RollbackController : ContentControllerExtenderBase
    {
        public RollbackController(IBackOfficeRequestContext backOfficeRequestContext)
            : base(backOfficeRequestContext)
        { }

        /// <summary>
        /// Rollback the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [HttpGet]
        [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.Rollback })]
        public virtual ActionResult Rollback(HiveId id)
        {
            var model = new RollbackModel { Id = id };

            using (var uow = Hive.Create<IContentStore>())
            {
                var entity = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(model.Id);
                if (entity == null)
                    throw new NullReferenceException("Could not find entity with id " + model.Id);

                model.LastRevision = entity;

                var nameAttr = model.LastRevision.Item.Attributes.FirstOrDefault(x => x.AttributeDefinition.Alias == NodeNameAttributeDefinition.AliasValue);
                model.Name = nameAttr != null ? nameAttr.GetValueAsString("Name") : "Unknown";
                model.CreateDate = model.LastRevision.Item.UtcStatusChanged.DateTime;

                var revisions = uow.Repositories.Revisions.GetAll<TypedEntity>(model.LastRevision.Item.Id);

                model.Versions = revisions
                    .OrderByDescending(x => x.MetaData.UtcStatusChanged)
                    .Skip(1) // Skip current revision
                    .Select(x =>
                        {
                            var name = x.MetaData.StatusType.Name.IfNull(y => new LocalizedString(x.MetaData.StatusType.Alias));

                            return new SelectListItem
                                         {
                                             Text = x.MetaData.UtcCreated.DateTime.ToShortDateString() + " " + x.MetaData.UtcCreated.DateTime.ToShortTimeString() + " - '" + name + "'", 
                                             Value = x.MetaData.Id.ToString()
                                         };
                        });
            }

            return View(model);
        }

        /// <summary>
        /// Rollback the form.
        /// </summary>
        /// <returns></returns>
        [ActionName("Rollback")]
        [HttpPost]
        //[RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.Rollback })] 
        public virtual JsonResult RollbackForm(HiveId id, HiveId revisionId)
        {
            using (var uow = Hive.Create<IContentStore>())
            {
                var entity = uow.Repositories.Get<TypedEntity>(id);
                if (entity == null)
                    throw new NullReferenceException("Could not find entity with id " + id);

                var revision = uow.Repositories.Revisions.Get<TypedEntity>(id, revisionId);
                if (revision == null)
                    throw new NullReferenceException("Could not find revision with id " + revisionId);

                var newRevision = revision.CopyToNewRevision();

                uow.Repositories.Revisions.AddOrUpdate(newRevision);
                uow.Complete();

                var successMsg = "Rollback.Success.Message".Localize(this, new
                    {
                        Name = entity.GetAttributeValue(NodeNameAttributeDefinition.AliasValue, "Name")
                    });

                Notifications.Add(new NotificationMessage(
                                      successMsg,
                                      "Rollback.Title".Localize(this), NotificationType.Success));

                return new CustomJsonResult(new
                    {
                        success = true,
                        notifications = Notifications,
                        msg = successMsg
                    }.ToJsonString);
            }
        }

        /// <summary>
        /// Returns a diff of typed entities current revision, and the revision supplied.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="revisionId">The revision id.</param>
        /// <returns></returns>
        [HttpPost]
        public virtual JsonResult Diff(HiveId id, HiveId revisionId)
        {
            using (var uow = Hive.Create<IContentStore>())
            {
                var entity = uow.Repositories.Get<TypedEntity>(id);
                if (entity == null)
                    throw new NullReferenceException("Could not find entity with id " + id);

                var revision = uow.Repositories.Revisions.Get<TypedEntity>(id, revisionId);
                if (revision == null)
                    throw new NullReferenceException("Could not find revision with id " + revisionId);

                var properties = new List<KeyValuePair<string, string>>();

                // Deal with name an create date specifically
                var entityNameAttr = entity.Attributes.FirstOrDefault(x => x.AttributeDefinition.Alias == NodeNameAttributeDefinition.AliasValue);
                var entityName = entityNameAttr != null ? entityNameAttr.GetValueAsString("Name") : "Unknown";

                var revisionNameAttr = revision.Item.Attributes.FirstOrDefault(x => x.AttributeDefinition.Alias == NodeNameAttributeDefinition.AliasValue);
                var revisionName = revisionNameAttr != null ? revisionNameAttr.GetValueAsString("Name") : "Unknown";

                // Fudge some differences
                // revisionName += "2";

                properties.Add(new KeyValuePair<string, string>("Name", new HtmlDiff(entityName, revisionName).Build()));

                var entityCreateDate = entity.UtcCreated.DateTime.ToShortDateString() + " " + entity.UtcCreated.DateTime.ToShortTimeString();
                var revisionCreateDate = revision.Item.UtcCreated.DateTime.ToShortDateString() + " " + revision.Item.UtcCreated.DateTime.ToShortTimeString();

                properties.Add(new KeyValuePair<string, string>("Created", new HtmlDiff(entityCreateDate, revisionCreateDate).Build()));

                // Now handle all, non system, attributes
                foreach(var attr in entity.Attributes.Where(x => !x.AttributeDefinition.Alias.StartsWith("system-")))
                {
                    var entityAttr = attr;
                    var revisionAttr = revision.Item.Attributes.SingleOrDefault(x => x.AttributeDefinition.Alias == attr.AttributeDefinition.Alias);

                    var entityValue = entityAttr.DynamicValue.ToString();
                    var revisionValue = revisionAttr != null ? revisionAttr.DynamicValue.ToString() : "";

                    properties.Add(new KeyValuePair<string, string>(attr.AttributeDefinition.Name, new HtmlDiff(entityValue, revisionValue).Build()));
                }

                //TODO: Check there aren't any attributes on the target revision that aren't on current?

                return new CustomJsonResult(properties.ToJsonString);
            }
        }
    }
}