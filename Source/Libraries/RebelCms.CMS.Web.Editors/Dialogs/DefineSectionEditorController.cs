using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Framework;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Hive;
using RebelCms.Hive.ProviderGrouping;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Cms.Web.Editors.Dialogs
{
    [Editor(CorePluginConstants.DefineSectionEditorControllerId)]
    [RebelCmsEditor]
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
