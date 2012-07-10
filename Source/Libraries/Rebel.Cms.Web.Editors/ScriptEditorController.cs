using System;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Mvc.ActionFilters;

using Rebel.Framework.Localization;
using Rebel.Hive.Configuration;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Editors
{
    [Editor(CorePluginConstants.ScriptEditorControllerId)]
    [RebelEditor]
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
