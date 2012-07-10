using System.Linq;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Security;
using Rebel.Cms.Web.Mvc.Controllers.BackOffice;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Security;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Hive;
using Rebel.Hive.RepositoryTypes;
using FixedHiveIds = Rebel.Framework.Security.Model.FixedHiveIds;

namespace Rebel.Cms.Web.Editors
{
    [Editor(CorePluginConstants.UserEditorControllerId)]
    [RebelEditor]
    [SupportClientNotifications]
    public class UserEditorController : AbstractUserEditorController<User, UserEditorModel>
    {
        public UserEditorController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        { }

        public override IMembershipService<User> MembershipService
        {
            get { return BackOfficeRequestContext.Application.Security.Users; }
        }

        public override string GroupProviderGroupRoot
        {
            get { return "security://user-groups"; }
        }

        public override HiveId ProfileVirtualRoot
        {
            get { return FixedHiveIds.MasterUserProfileSchema; }
        }

        public override string CreateNewTitle
        {
            get { return "Create new user"; }
        }

        protected override void GeneratePathsForCurrentEntity(User entity)
        {
            //add path for entity for SupportsPathGeneration (tree syncing) to work
            GeneratePathsForCurrentEntity(new EntityPathCollection(entity.Id, new[]{ new EntityPath(new[]
                {
                    Framework.Security.Model.FixedHiveIds.UserVirtualRoot, 
                    entity.Id, 
                })
            }));
        }

        protected override void OnAfterSave(User entity)
        {
            //we may have changed the user data, so we need to ensure that the latest user data exists in the Identity object so we'll re-issue a forms auth ticket here
            if (HttpContext.User.Identity.Name.InvariantEquals(entity.Username))
            {
                var userData = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<UserData>(entity);
                HttpContext.CreateRebelAuthTicket(userData);
            }

            base.OnAfterSave(entity);
        }

        protected override void EnsureViewBagData()
        {
            var uowFactory = BackOfficeRequestContext.Application.Hive.GetReader<ISecurityStore>();
            using (var uow = uowFactory.CreateReadonly())
            {
                ViewBag.AvailableUserGroups = uow.Repositories.GetChildren<UserGroup>(
                    FixedRelationTypes.DefaultRelationType, Framework.Security.Model.FixedHiveIds.UserGroupVirtualRoot)
                    .OrderBy(x => x.Name);
            }

            ViewBag.AvailableApplications = BackOfficeRequestContext.Application.Settings.Applications;
        }
    }
}
