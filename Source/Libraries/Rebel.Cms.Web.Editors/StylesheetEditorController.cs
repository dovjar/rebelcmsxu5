using System;
using System.Text;
using System.Web.Mvc;
using System.Linq;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.IO;
using Rebel.Cms.Web.Model;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Cms.Web.Mvc.ViewEngines;
using Rebel.Framework;
using Rebel.Hive;
using Rebel.Framework.Localization;
using Rebel.Hive.Configuration;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Editors
{
    [Editor(CorePluginConstants.StylesheetEditorControllerId)]
    [RebelEditor]
    [SupportClientNotifications]
    [AlternateViewEnginePath("ScriptEditor")]
    public class StylesheetEditorController : AbstractFileEditorController
    {
        private readonly GroupUnitFactory<IFileStore> _hive;

        public StylesheetEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
            _hive = BackOfficeRequestContext
                .Application
                .Hive
                .GetWriter<IFileStore>(new Uri("storage://stylesheets"));
        }

        public override GroupUnitFactory<IFileStore> Hive
        {
            get { return _hive; }
        }

        protected override string SaveSuccessfulTitle
        {
            get { return "Stylesheet.Save.Title".Localize(this); }
        }

        protected override string SaveSuccessfulMessage
        {
            get { return "Stylesheet.Save.Message".Localize(this); }
        }

        protected override string CreateNewTitle
        {
            get { return "Create a stylesheet"; }
        }

        protected override string[] AllowedFileExtensions
        {
            get { return new[] { ".css" }; }
        }

        [HttpGet]
        public ActionResult EditRule(HiveId id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            //The rule Id consists of both the stylesheet Id and the rule name
            var idParts = id.StringParts().ToList();
            var stylesheetId = idParts[0];
            var ruleName = idParts.Count > 1 ? idParts[1] : "";

            ViewBag.IsNew = string.IsNullOrWhiteSpace(ruleName);

            using (var uow = _hive.Create())
            {
                var stylesheet = uow.Repositories.Get<Rebel.Framework.Persistence.Model.IO.File>(new HiveId(stylesheetId));
                var rule = !string.IsNullOrWhiteSpace(ruleName) ?
                    StylesheetHelper.ParseRules(stylesheet).Single(x => x.Name.Replace(" ", "__s__") == ruleName) :
                    new StylesheetRule() { StylesheetId = id };

                var ruleModel = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<StylesheetRuleEditorModel>(rule);

                return View(ruleModel);
            }
        }

        [HttpPost]
        [SupportsPathGeneration]
        [Save]
        public ActionResult EditRule(StylesheetRuleEditorModel ruleModel)
        {
            Mandate.ParameterNotNull(ruleModel, "rule");

            if (!TryValidateModel(ruleModel))
            {
                return View(ruleModel);
            }

            using (var uow = _hive.Create())
            {
                var repo = uow.Repositories;
                var stylesheet = repo.Get<Rebel.Framework.Persistence.Model.IO.File>(ruleModel.ParentId);
                var rule = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<StylesheetRule>(ruleModel);

                if (!ruleModel.Id.IsNullValueOrEmpty())
                {
                    var idParts = ruleModel.Id.StringParts().ToList();
                    if (idParts.Count == 2)
                    {
                        var oldRuleName = idParts[1].Replace("__s__", " ");

                        StylesheetHelper.ReplaceRule(stylesheet, oldRuleName, rule);
                    }
                    else
                    {
                        StylesheetHelper.AppendRule(stylesheet, rule);
                    }
                }
                else
                {
                    StylesheetHelper.AppendRule(stylesheet, rule);
                }

                repo.AddOrUpdate(stylesheet);
                uow.Complete();

                //Always update the Id to be based on the name
                ruleModel.Id = new HiveId(new Uri("storage://stylesheets"), string.Empty, new HiveIdValue(ruleModel.ParentId.Value + "/" + ruleModel.Name.Replace(" ", "__s__")));
            }

            Notifications.Add(new NotificationMessage("Rule saved", NotificationType.Success));

            //add path for entity for SupportsPathGeneration (tree syncing) to work,
            //we need to manually create this path:            
            GeneratePathsForCurrentEntity(new EntityPathCollection(ruleModel.Id, new[]{ new EntityPath(new[]
                {
                    _hive.GetRootNodeId(),
                    ruleModel.ParentId,
                    ruleModel.Id
                })
            }));

            return RedirectToAction("EditRule", new { id = ruleModel.Id });
        }

        [HttpDelete]
        public ActionResult DeleteRule(HiveId id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            var idParts = id.StringParts().ToList();
            var stylesheetId = idParts[0];
            var ruleName = idParts[1];

            using (var uow = _hive.Create())
            {
                var repo = uow.Repositories;
                var stylesheet = repo.Get<Rebel.Framework.Persistence.Model.IO.File>(new HiveId(stylesheetId));

                StylesheetHelper.ReplaceRule(stylesheet, ruleName, null);

                repo.AddOrUpdate(stylesheet);
                uow.Complete();
            }

            //return a successful JSON response
            return Json(new { message = "Success" });
        }
    }
}