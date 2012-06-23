using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework;
using RebelCms.Framework.Localization;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Hive;
using RebelCms.Hive.ProviderGrouping;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Cms.Web.Editors
{
    [Editor(CorePluginConstants.DocumentTypeEditorControllerId)]
    [RebelCmsEditor]
    [SupportClientNotifications]
    public class DocumentTypeEditorController : AbstractSchemaEditorController<DocumentTypeEditorModel, CreateDocumentTypeModel>
    {

        public DocumentTypeEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        protected override string CreateNewTitle
        {
            get { return "Create a new document type"; }
        }

        public override GroupUnitFactory Hive
        {
            get { return BackOfficeRequestContext.Application.Hive.GetWriter<IContentStore>(); }
        }

        protected override HiveId RootSchema
        {
            get { return FixedHiveIds.ContentRootSchema; }
        }

        public override void OnBeforeCreate(CreateDocumentTypeModel createModel, DocumentTypeEditorModel editorModel)
        {
            base.OnBeforeCreate(createModel, editorModel);

            if (createModel.CreateTemplate)
            {
                var templateEditor = new TemplateEditorController(BackOfficeRequestContext)
                {
                    ControllerContext = this.ControllerContext
                };

                var templateModel = templateEditor.CreateNewEditorModel(editorModel.Alias + ".cshtml",
                    HiveId.Empty,
                    new HiveId(new Uri("storage://stubs"), "stubs", new HiveIdValue("TemplateEditor\\Clean.cshtml")));

                //TODO: Should check whether one with same name already exists, currently overwrites
                var file = templateEditor.PerformSave(templateModel);
                editorModel.AllowedTemplateIds.Add(file.Id);
                editorModel.DefaultTemplateId = file.Id;

                //repopulate collections
                EnsureSelectListData(editorModel);
            }
        }

        protected override DocumentTypeEditorModel CreateNewEditorModel(string name = null, Tab genericTab = null)
        {
            var model = new DocumentTypeEditorModel();
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

        /// <summary>
        /// Ensures the select lists are built for the model
        /// </summary>
        /// <param name="model"></param>
        protected override void EnsureSelectListData(DocumentTypeEditorModel model)
        {
            base.EnsureSelectListData(model);

            using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<IFileStore>(new Uri("storage://templates")))
            {
                //create the allowed templates check box list
                model.AllowedTemplates = new List<SelectListItem>(
                    uow.Repositories.GetAllNonContainerFiles()
                    .OrderBy(x => x.Name)
                    .Where(x => !x.Name.StartsWith("_")) // According to Microsoft standards, files prefixed with underscore should not be accessed directly, so don't show as usable templates
                    .Select(x =>
                        new SelectListItem
                        {
                            Text = x.GetFileNameForDisplay(),
                            Value = x.Id.ToString(),
                            Selected = model.AllowedTemplateIds.Contains(x.Id)
                        })).ToArray();
            }

        }

    }
}
