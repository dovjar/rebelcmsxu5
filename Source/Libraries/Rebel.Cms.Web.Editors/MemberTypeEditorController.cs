using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Cms.Web.Mvc.ViewEngines;

using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Hive.RepositoryTypes;
using FixedHiveIds = Rebel.Framework.Security.Model.FixedHiveIds;

namespace Rebel.Cms.Web.Editors
{

    [Editor(CorePluginConstants.MemberTypeEditorControllerId)]
    [RebelEditor]
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
                model.Alias = name.ToRebelAlias();
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