using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.ViewEngines;

using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive.RepositoryTypes;
using FixedHiveIds = Umbraco.Framework.Security.Model.FixedHiveIds;

namespace Umbraco.Cms.Web.Editors
{

    [Editor(CorePluginConstants.MemberTypeEditorControllerId)]
    [UmbracoEditor]
    [SupportClientNotifications]
    [AlternateViewEnginePath("DocumentTypeEditor")]
    public class MemberTypeEditorController : AbstractSchemaEditorController<ProfileTypeEditorModel, CreateContentModel>
    {
        public MemberTypeEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        public override Hive.ProviderGrouping.GroupUnitFactory Hive
        {
            get { return BackOfficeRequestContext.Application.Hive.GetWriter(new Uri("security://member-types")); }
        }

        protected override string CreateNewTitle
        {
            get { return "Create a new member type"; }
        }

        protected override HiveId RootSchema
        {
            get { return FixedHiveIds.MasterMemberProfileSchema; }
        }

        protected override ProfileTypeEditorModel CreateNewEditorModel(string name = null, Tab genericTab = null)
        {
            var model = new ProfileTypeEditorModel();
            if (!name.IsNullOrWhiteSpace())
            {
                model.Alias = name.ToUmbracoAlias();
                model.Name = name;
            }
            if (genericTab != null)
            {
                model.DefinedTabs = new HashSet<Tab>(new[] { genericTab });
                model.AvailableTabs = new SelectList(new[] { genericTab }, "Id", "Name");
            }
            return model;
        }    
    }
}