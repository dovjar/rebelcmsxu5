using System;
using System.Linq;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Cms.Web.Mvc;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Cms.Web.Mvc.ViewEngines;
using RebelCms.Framework;
using RebelCms.Framework.Localization;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Persistence.Model.Constants.AttributeDefinitions;
using RebelCms.Framework.Security;
using RebelCms.Hive;
using RebelCms.Hive.ProviderGrouping;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Cms.Web.Editors.Extenders
{
    /// <summary>
    /// Used for sorting content
    /// </summary>    
    public class SortController : ContentControllerExtenderBase
    {
        public SortController(IBackOfficeRequestContext backOfficeRequestContext)
            : base(backOfficeRequestContext)
        {
        }

        /// <summary>
        /// Displays the sort dialog
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.Sort })] 
        public virtual ActionResult Sort(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();

            using (var uow = Hive.Create<IContentStore>())
            {
                var exists = uow.Repositories.Exists<TypedEntity>(id.Value);
                if (!exists)
                    throw new ArgumentException(string.Format("No entity found for id: {0} on action Sort", id));

                var model = new SortModel { ParentId = id.Value };

                var items = uow.Repositories.GetLazyChildRelations(id.Value, FixedRelationTypes.DefaultRelationType);

                model.Items = items.Select(
                    x => new SortItem
                        {
                            UtcCreated = x.Destination.UtcCreated,
                            Id = x.Destination.Id,
                            SortIndex = x.Ordinal,
                            //TODO: Casting the relation as a TPE here but the item may be related to something else, not a TPE: need a helper method for returning the name
                            Name =
                                ((TypedEntity)x.Destination).Attributes[NodeNameAttributeDefinition.AliasValue].Values[
                                    "Name"].ToString()
                        })
                    .OrderBy(x => x.SortIndex)
                    .ToArray();

                return View(model);
            }
        }

        /// <summary>
        /// Handles the ajax request for the publish dialog
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ActionName("Sort")]
        [HttpPost]
        //[RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.Sort })] 
        public virtual JsonResult SortForm(SortModel model)
        {
            if (!TryValidateModel(model))
            {
                return ModelState.ToJsonErrors();
            }

            using (var uow = Hive.Create<IContentStore>())
            {
                var found = uow.Repositories.Get(model.ParentId);
                var exists = uow.Repositories.Exists<TypedEntity>(model.ParentId);
                if (!exists)
                    throw new ArgumentException(string.Format("No entity found for id: {0} on action Sort", model.ParentId));

                var childRelations = uow.Repositories.GetChildRelations(model.ParentId, FixedRelationTypes.DefaultRelationType);

                foreach (var item in model.Items)
                {
                    var relation = childRelations.Single(x => x.DestinationId.EqualsIgnoringProviderId(item.Id));
                    uow.Repositories.ChangeRelation(relation, relation.SourceId, relation.DestinationId, item.SortIndex);
                }

                uow.Complete();
            }

            Notifications.Add(new NotificationMessage(
                                  "Sort.Success.Message".Localize(this),
                                  "Sort.Title".Localize(this), NotificationType.Success));
            return new CustomJsonResult(new
                {
                    success = true,
                    notifications = Notifications,
                    msg = "Sort.Success.Message".Localize(this)
                }.ToJsonString);
        }
    }
}