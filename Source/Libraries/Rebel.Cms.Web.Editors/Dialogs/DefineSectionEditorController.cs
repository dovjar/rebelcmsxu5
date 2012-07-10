using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Hive;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Editors.Dialogs
{
    [Editor(CorePluginConstants.DefineSectionEditorControllerId)]
    [RebelEditor]
    public class DefineSectionEditorController : AbstractEditorController
    {
        public DefineSectionEditorController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        { }

        [HttpGet]
        public ActionResult Index(HiveId id)
        {
            return View();
        }
    }
}
