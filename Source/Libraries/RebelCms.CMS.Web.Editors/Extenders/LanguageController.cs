using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Cms.Web.Mvc;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Cms.Web.Mvc.ViewEngines;
using RebelCms.Framework;
using RebelCms.Framework.Localization;
using RebelCms.Framework.Persistence;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Associations;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Persistence.Model.Constants.AttributeDefinitions;
using RebelCms.Framework.Persistence.Model.Constants.Entities;
using RebelCms.Framework.Persistence.Model.Constants.Schemas;
using RebelCms.Framework.Security;
using RebelCms.Hive;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Cms.Web.Editors.Extenders
{
    /// <summary>
    /// Used for the language dialog
    /// </summary>
    public class LanguageController : ContentControllerExtenderBase
    {
        public LanguageController(IBackOfficeRequestContext backOfficeRequestContext)
            : base(backOfficeRequestContext)
        {
        }

        [HttpGet]
        [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.Hostnames })] 
        public virtual ActionResult Language(HiveId? id)
        {
            if (id.IsNullValueOrEmpty())
                return HttpNotFound();

            // TODO: Check for a current language
            using (var uow = Hive.Create<IContentStore>())
            {
                //get the typed/content entity for which to assign hostnames
                var entity = uow.Repositories.Get<TypedEntity>(id.Value);
                if (entity == null)
                    throw new ArgumentException("Could not find entity with id " + id);

                //get the assigned hostnames
                var languageRelations = uow.Repositories.GetParentRelations(id.Value, FixedRelationTypes.LanguageRelationType);
                var language = languageRelations.Any() ? languageRelations.Single().MetaData.SingleOrDefault(x => x.Key == "IsoCode").Value : null;

                return View(new LanguageModel
                {
                    Id = id.Value,
                    IsoCode = language,
                    InstalledLanguages = BackOfficeRequestContext.Application.Settings.Languages
                        .Where(x => x.IsoCode != id.Value.ToString())
                        .OrderBy(x => x.Name)
                        .Select(x => new SelectListItem
                        {
                            Text = x.Name,
                            Value = x.IsoCode
                        }).ToList()
                });
            }
        }

        [ActionName("Language")]
        [HttpPost]
        public virtual JsonResult LanguageForm(LanguageModel model)
        {
            Mandate.ParameterNotEmpty(model.Id, "Id");

            using (var uow = Hive.Create<IContentStore>())
            {
                //get the content entity for the language assignment
                var entity = uow.Repositories.Get<TypedEntity>(model.Id);
                if (entity == null)
                    throw new NullReferenceException("Could not find entity with id " + model.Id);

                var languageRelations = uow.Repositories.GetParentRelations(model.Id, FixedRelationTypes.LanguageRelationType);
                var languageRelation = languageRelations.SingleOrDefault();

                if(model.IsoCode.IsNullOrWhiteSpace())
                {
                    if (languageRelation != null)
                        uow.Repositories.RemoveRelation(languageRelation);
                }
                else
                {
                    if (languageRelation == null || languageRelation.MetaData.SingleOrDefault(x => x.Key == "IsoCode").Value != model.IsoCode)
                    {
                        var metaData = new List<RelationMetaDatum>
                        {
                            new RelationMetaDatum("IsoCode", model.IsoCode)
                        };

                        uow.Repositories.ChangeOrCreateRelationMetadata(FixedHiveIds.SystemRoot, entity.Id,
                            FixedRelationTypes.LanguageRelationType, metaData.ToArray());
                    }
                }

                uow.Complete();

                //clears the domain cache
                //BackOfficeRequestContext.RoutingEngine.ClearCache(clearDomains: true, clearGeneratedUrls: true);

                var successMsg = "Language.Success.Message".Localize(this, new
                {
                    Name = entity.GetAttributeValue(NodeNameAttributeDefinition.AliasValue, "Name")
                });
                Notifications.Add(new NotificationMessage(successMsg, "Language.Title".Localize(this), NotificationType.Success));

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
