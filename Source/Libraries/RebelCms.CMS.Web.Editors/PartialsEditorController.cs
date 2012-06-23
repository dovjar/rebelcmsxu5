using System;
using System.Collections.Generic;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Cms.Web.Model.BackOffice.UIElements;
using RebelCms.Cms.Web.Mvc.ActionFilters;

using RebelCms.Framework.Localization;
using RebelCms.Hive.Configuration;
using RebelCms.Hive.ProviderGrouping;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Cms.Web.Editors
{
    [Editor(CorePluginConstants.PartialsEditorControllerId)]
    [RebelCmsEditor]
    [SupportClientNotifications]
    public class PartialsEditorController : AbstractFileEditorController
    {
        private readonly GroupUnitFactory<IFileStore> _hive;

        public PartialsEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
            _hive = BackOfficeRequestContext
                .Application
                .Hive
                .GetWriter<IFileStore>(new Uri("storage://partials"));
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
            get { return "Partials.Save.Title".Localize(this); }
        }

        protected override string SaveSuccessfulMessage
        {
            get { return "Partials.Save.Message".Localize(this); }
        }

        protected override string CreateNewTitle
        {
            get { return "Create a partial"; }
        }

        protected override string[] AllowedFileExtensions
        {
            get { return new[] {".cshtml"}; }
        }

        protected override void EnsureViewData(FileEditorModel model, Framework.Persistence.Model.IO.File file)
        {
            base.EnsureViewData(model, file);

            // Setup UIElements
            model.UIElements.Add(new SeperatorUIElement());
            model.UIElements.Add(new ButtonUIElement
            {
                Alias = "InsertField",
                Title = "Insert an RebelCms page field",
                CssClass = "insert-field-button toolbar-button",
                AdditionalData = new Dictionary<string, string>
                {
                    { "id", "submit_InsertField" },
                    { "name", "submit.InsertField" }
                }
            });
            model.UIElements.Add(new ButtonUIElement
            {
                Alias = "InsertMacro",
                Title = "Insert a macro",
                CssClass = "insert-macro-button toolbar-button",
                AdditionalData = new Dictionary<string, string>
                {
                    { "id", "submit_InsertMacro" },
                    { "name", "submit.InsertMacro" }
                }
            });
        }
    }
}
