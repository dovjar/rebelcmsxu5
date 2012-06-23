using System;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Cms.Web.Mvc.ActionFilters;

using RebelCms.Framework.Localization;
using RebelCms.Hive.Configuration;
using RebelCms.Hive.ProviderGrouping;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Cms.Web.Editors
{
    [Editor(CorePluginConstants.ScriptEditorControllerId)]
    [RebelCmsEditor]
    [SupportClientNotifications]
    public class ScriptEditorController : AbstractFileEditorController
    {
        private readonly GroupUnitFactory<IFileStore> _hive;

        public ScriptEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
            _hive = BackOfficeRequestContext
                .Application
                .Hive
                .GetWriter<IFileStore>(new Uri("storage://scripts"));
        }

        public override GroupUnitFactory<IFileStore> Hive
        {
            get
            {
                return _hive;
            }
        }

        protected override string SaveSuccessfulTitle
        {
            get { return "Script.Save.Title".Localize(this); }
        }

        protected override string SaveSuccessfulMessage
        {
            get { return "Script.Save.Message".Localize(this); }
        }

        protected override string CreateNewTitle
        {
            get { return "Create a script"; }
        }

        protected override string[] AllowedFileExtensions
        {
            get { return new[] {".js"}; }
        }
    }
}
