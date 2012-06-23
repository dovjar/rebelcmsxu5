using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Cms.Web.Templates;
using RebelCms.Framework;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Persistence.Model.IO;
using RebelCms.Hive;
using RebelCms.Hive.ProviderGrouping;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Cms.Web.Editors.Dialogs
{
    [Editor(CorePluginConstants.InsertPartialEditorControllerId)]
    [RebelCmsEditor]
    public class InsertPartialEditorController : AbstractEditorController
    {
        private readonly ReadonlyGroupUnitFactory<IFileStore> _partialsStore;

        public InsertPartialEditorController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        {
            _partialsStore = BackOfficeRequestContext
                .Application
                .Hive
                .GetReader<IFileStore>(new Uri("storage://partials"));
        }

        [HttpGet]
        public ActionResult Index(HiveId id)
        {
            var model = new InsertPartialModel { AvailablePartials = Enumerable.Empty<SelectListItem>() };

            using (var uow = _partialsStore.CreateReadonly())
            {
                var rootId = _partialsStore.GetRootNodeId();
                var root = uow.Repositories.Get<File>(rootId);
                var partialIds = uow.Repositories.GetDescendantIds(_partialsStore.GetRootNodeId(), FixedRelationTypes.DefaultRelationType);
                var partials = uow.Repositories.Get<File>(true, partialIds).Where(x => !x.IsContainer);

                var availablePartials = new List<SelectListItem>();
                foreach (var @partial in partials)
                {
                    availablePartials.Add(new SelectListItem{ Text = @partial.GetFilePathForDisplay(), Value = @partial.GetFilePathWithoutExtension()});
                }

                model.AvailablePartials = availablePartials;
            }

            return View(model);
        }
    }
}
