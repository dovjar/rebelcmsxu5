using System;
using RebelCms.Cms.Web;
using RebelCms.Framework;
using RebelCms.Framework.Context;
using RebelCms.Framework.Diagnostics;
using RebelCms.Framework.Persistence;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Tasks;
using RebelCms.Hive;
using RebelCms.Hive.RepositoryTypes;
using RebelCms.Hive.Tasks;

namespace RebelCms.Cms.Packages.DevDataset
{
    [Task("FA00809A-242F-439D-94E9-A248663BC4E2", TaskTriggers.PostPackageInstall, ContinueOnFailure = false)]
    public class DevDatasetInstallTask : HiveProviderInstallTask
    {
        private readonly DevDataset _devDataSet;

        public DevDatasetInstallTask(
            IFrameworkContext frameworkContext,
            IPropertyEditorFactory propertyEditorFactory,
            IHiveManager hiveManager,
            IAttributeTypeRegistry attributeTypeRegistry)
            : base(frameworkContext, hiveManager)
        {
            _devDataSet = new DevDataset(propertyEditorFactory, frameworkContext, attributeTypeRegistry);
        }

        public override bool NeedsInstallOrUpgrade
        {
            get
            {
                var homePageId = HiveId.ConvertIntToGuid(1048);
                try
                {
                    using (var uow = CoreManager.OpenReader<IContentStore>())
                    {
                        var hasPage = uow.Repositories.Exists<TypedEntity>(homePageId);
                        if (!hasPage)
                            return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    LogHelper.Error<DevDatasetInstallTask>(
                        string.Format("Error checking if {0} exists", homePageId), ex);
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
            _devDataSet.InstallDevDataset(CoreManager, Context);
        }
    }
}