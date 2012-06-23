using System;
using System.Collections.Generic;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Cms.Web.Mvc.ViewEngines;

using RebelCms.Framework;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Cms.Web.Editors
{

    [Editor(CorePluginConstants.MediaTypeEditorControllerId)]
    [RebelCmsEditor]
    [SupportClientNotifications]
    [AlternateViewEnginePath("DocumentTypeEditor")]
    public class MediaTypeEditorController : AbstractSchemaEditorController<MediaTypeEditorModel, CreateContentModel>
    {
        public MediaTypeEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        public override Hive.ProviderGrouping.GroupUnitFactory Hive
        {
            get { return BackOfficeRequestContext.Application.Hive.GetWriter(new Uri("media://")); }
        }

        protected override string CreateNewTitle
        {
            get { return "Create a new media type"; }
        }

        protected override HiveId RootSchema
        {
            get { return FixedHiveIds.MediaRootSchema; }
        }

        protected override MediaTypeEditorModel CreateNewEditorModel(string name = null, Tab genericTab = null)
        {
            var model = new MediaTypeEditorModel();
            if (!name.IsNullOrWhiteSpace())
            {
                model.Alias = name.ToRebelCmsAlias();
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