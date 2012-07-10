using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Templates;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.IO;
using Rebel.Hive;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Editors.Dialogs
{
    [Editor(CorePluginConstants.ImplementSectionEditorControllerId)]
    [RebelEditor]
    public class ImplementSectionEditorController : AbstractEditorController
    {
        private readonly ReadonlyGroupUnitFactory<IFileStore> _templateStore;

        public ImplementSectionEditorController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        {
            _templateStore = BackOfficeRequestContext
                .Application
                .Hive
                .GetReader<IFileStore>(new Uri("storage://templates"));
        }

        [HttpGet]
        public ActionResult Index(HiveId id)
        {
            var model = new ImplementSectionModel { AvailableSections = Enumerable.Empty<string>() };

            using (var uow = _templateStore.CreateReadonly())
            {
                var template = uow.Repositories.Get<File>(id);
                if(template != null)
                {
                    var parser = new TemplateParser();
                    var result = parser.Parse(template);
                    if (!string.IsNullOrWhiteSpace(result.Layout))
                    {
                        var layoutFilePath = TemplateHelper.ResolveLayoutPath(result.Layout, template);
                        var layoutFile = uow.Repositories.GetAll<File>().Where(x => x.RootedPath == layoutFilePath).FirstOrDefault();
                        var layoutResult = parser.Parse(layoutFile);

                        model.AvailableSections = layoutResult.Sections.Where(x => x != "Body").ToArray();
                    }
                }
            }

            return View(model);
        }
    }
}
