using System;
using System.Linq;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Cms.Web.Mvc.ViewEngines;
using Rebel.Cms.Web.Security;
using Rebel.Framework;
using Rebel.Framework.Localization;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Associations;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Security;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Hive;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;
using FixedHiveIds = Rebel.Framework.Security.Model.FixedHiveIds;

namespace Rebel.Cms.Web.Editors
{
    [Editor(CorePluginConstants.MemberEditorControllerId)]
    [RebelEditor]
    [SupportClientNotifications]
    [AlternateViewEnginePath("UserEditor")]
    public class MemberEditorController : AbstractUserEditorController<Member, MemberEditorModel>
    {
        public MemberEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        { }

        public override IMembershipService<Member> MembershipService
        {
            get { return BackOfficeRequestContext.Application.Security.Members; }
        }

        public override string GroupProviderGroupRoot
        {
            get { return "security://member-groups"; }
        }

        public override HiveId ProfileVirtualRoot
        {
            get { return FixedHiveIds.MasterMemberProfileSchema; }
        }

        public override string CreateNewTitle
        {
            get { return "Create new member"; }
        }

        protected override void GeneratePathsForCurrentEntity(Member entity)
        {
            //add path for entity for SupportsPathGeneration (tree syncing) to work
            GeneratePathsForCurrentEntity(new EntityPathCollection(entity.Id, new[]{ new EntityPath(new[]
                {
                    Framework.Security.Model.FixedHiveIds.MemberVirtualRoot, 
                    new HiveId(new Uri("security://members"), "", new HiveIdValue(entity.Username.Substring(0, 1).ToUpper())),
                    entity.Id, 
                })
            }));
        }

        protected override void EnsureViewBagData()
        {
            var uowFactory = BackOfficeRequestContext.Application.Hive.GetReader<ISecurityStore>();
            using (var uow = uowFactory.CreateReadonly())
            {
                ViewBag.AvailableUserGroups = uow.Repositories.GetChildren<UserGroup>(
                    FixedRelationTypes.DefaultRelationType, Framework.Security.Model.FixedHiveIds.MemberGroupVirtualRoot)
                    .OrderBy(x => x.Name);
            }

            ViewBag.AvailableApplications = BackOfficeRequestContext.Application.Settings.Applications;
        }

        [HttpGet]
        public ActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public JsonResult SearchForm(string searchTerm)
        {
            var members = BackOfficeRequestContext.Application.Security.Members.FindByUsername(searchTerm + "*");

            return Json(new
            {
                members = members.Select(x => new {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Username = x.Username,
                    Email = x.Email
                })
            });
        }
    }
}