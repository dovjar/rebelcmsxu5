using System;
using System.Collections.Generic;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Model.BackOffice.UIElements;
using Rebel.Cms.Web.Mvc.ActionFilters;

using Rebel.Framework.Localization;
using Rebel.Hive.Configuration;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Editors
{
    [Editor(CorePluginConstants.PartialsEditorControllerId)]
    [RebelEditor]
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
                Title = "Insert an rebel page field",
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
