using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Security;
using Rebel.Cms.Web.System;
using Rebel.Framework;
using Rebel.Framework.Context;
using Rebel.Framework.Diagnostics;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Associations;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Security;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Framework.Tasks;
using Rebel.Hive;
using Rebel.Hive.RepositoryTypes;
using Rebel.Hive.Tasks;
using FixedHiveIds = Rebel.Framework.Security.Model.FixedHiveIds;

namespace Rebel.Cms.Web.Tasks
{
    /// <summary>
    ///   Once Hive is setup/installed, we need to get the core data into the system
    /// </summary>
    [Task("FD9FA9D5-F34A-4E1F-95A1-B08D6B00C4FC", TaskTriggers.Hive.PostInstall, ContinueOnFailure = false)]
    public class EnsureCoreDataTask : HiveProviderInstallTask
    {
        private readonly IEnumerable<Lazy<Permission, PermissionMetadata>> _permissions;
        private readonly ISecurityService _securityService;

        public EnsureCoreDataTask(IFrameworkContext context, IHiveManager coreManager, IEnumerable<Lazy<Permission, PermissionMetadata>> permissions,
            ISecurityService securityService)
            : base(context, coreManager)
        {
            _permissions = permissions;
            _securityService = securityService;
        }

        public override bool NeedsInstallOrUpgrade
        {
            get
            {
                var sysRoot = new SystemRoot();
                try
                {
                    using (var uow = CoreManager.OpenReader<IContentStore>())
                    {
                        var hasRoot = uow.Repositories.Exists<TypedEntity>(sysRoot.Id);
                        return !hasRoot;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error<EnsureCoreDataTask>(string.Format("Error checking if {0} exists", sysRoot.Id), ex);
                    return true;
                }
            }
        }

        public override int GetInstalledVersion()
        {
            return 0;
        }

        public override void InstallOrUpgrade()
        {
            //insert core components first
            using (var uow = CoreManager.OpenWriter<IContentStore>())
            {
                CoreCmsData.RequiredCoreRootNodes()
                    .ForEach(x => uow.Repositories.AddOrUpdate(x));

                CoreCmsData.RequiredCoreSchemas()
                    .ForEach(x => uow.Repositories.Schemas.AddOrUpdate(x));

                //user groups
                CoreCmsData.RequiredCoreUserGroups(_permissions).ForEach(
                    x =>
                        {
                            var existing = uow.Repositories.Query<UserGroup>().Any(y => y.Name == x.Name);

                            if (!existing)
                            {
                                x.RelationProxies.EnlistParentById(FixedHiveIds.UserGroupVirtualRoot, FixedRelationTypes.DefaultRelationType);
                                uow.Repositories.AddOrUpdate(x);
                            }
                        });

                //member groups
                //CoreCmsData.RequiredCoreMemberGroups(_permissions).ForEach(
                //    x =>
                //    {
                //        var existing = uow.Repositories.Query<UserGroup>().Any(y => y.Name == x.Name);

                //        if (!existing)
                //        {
                //            x.RelationProxies.EnlistParentById(FixedHiveIds.MemberGroupVirtualRoot, FixedRelationTypes.DefaultRelationType);
                //            uow.Repositories.AddOrUpdate(x);
                //        }
                //    });

                //attribute types, this also sets the RenderTypeProvider on the previously submitted attribute types
                CoreCmsData.RequiredCoreSystemAttributeTypes()
                    .ForEach(x => uow.Repositories.Schemas.AddOrUpdate(x));
                CoreCmsData.RequiredCoreUserAttributeTypes()
                    .ForEach(x => uow.Repositories.Schemas.AddOrUpdate(x));

                uow.Complete();
            }

            // We not longer add users as part of the ensure data task
            // instead the user is created as part of the installer (MB 2012/01/26)
            //using (var uow = CoreManager.OpenWriter<IContentStore>())
            //{
            //    // users
            //    CoreCmsData.RequiredCoreUsers()
            //        .ForEach(x =>
            //        {
            //            //find user group with same name as user
            //            var userGroup =
            //                uow.Repositories.GetEntityByRelationType<UserGroup>(
            //                    FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserGroupVirtualRoot)
            //                    .Where(y => y.Name == x.Name)
            //                    .FirstOrDefault();

            //            if (userGroup != null)
            //                x.RelationProxies.EnlistParent(userGroup, FixedRelationTypes.UserGroupRelationType);

            //            uow.Repositories.AddOrUpdate(x);

            //            _securityService.UsersMembershipService.CreateUser(x, out status);

            //            if (status != MembershipCreateStatus.Success)
            //                LogHelper.Error<EnsureCoreDataTask>(string.Format("Error creating user {0}", x.Username),
            //                    new ApplicationException(status.ToString()));

            //        });

            //    uow.Complete();
            //}

  
        }
    }
}