using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NSubstitute;
using RebelCms.Cms;
using RebelCms.Cms.Web;
using RebelCms.Cms.Web.Configuration;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Security;
using RebelCms.Cms.Web.Security.Permissions;
using RebelCms.Framework;
using RebelCms.Framework.Context;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Constants.Entities;
using RebelCms.Framework.Persistence.Model.Versioning;
using RebelCms.Framework.ProviderSupport;
using RebelCms.Framework.Security;
using RebelCms.Framework.Testing;
using RebelCms.Hive;
using RebelCms.Hive.RepositoryTypes;
using RebelCms.Tests.Extensions.Stubs;

namespace RebelCms.Tests.Extensions
{
    public class FakeRebelCmsApplicationContext : DisposableObject, IRebelCmsApplicationContext
    {
        //private readonly NHibernateInMemoryRepository _repo;

        public FakeRebelCmsApplicationContext(bool addSystemRooNode = true)
            : this(FakeHiveCmsManager.New(new FakeFrameworkContext()), addSystemRooNode)
        {
            
        }

        public FakeRebelCmsApplicationContext(IHiveManager hive, bool addSystemRooNode = true)
        {
            ApplicationId = Guid.NewGuid();

            //_repo = new NHibernateInMemoryRepository(cmsManager.CoreManager.FrameworkContext);

            Hive = hive;
            FrameworkContext = Hive.FrameworkContext;
            //Security = MockRepository.GenerateMock<ISecurityService>();
            Security = Substitute.For<ISecurityService>();
            Security.GetEffectivePermission(Arg.Any<Guid>(), Arg.Any<HiveId>(), Arg.Any<HiveId>())
                .Returns(new PermissionResult(new BackOfficeAccessPermission(), HiveId.Empty, PermissionStatus.Allow));
            Security.GetEffectivePermissions(Arg.Any<HiveId>(), Arg.Any<HiveId>(), Arg.Any<Guid[]>())
                .Returns(new PermissionResults(new PermissionResult(new BackOfficeAccessPermission(), HiveId.Empty, PermissionStatus.Allow)));


            if (addSystemRooNode)
            {
                //we need to add the root node
                // Create root node
                var root = new SystemRoot();
                AddPersistenceData(root);
            }

            //get the bin folder
            var binFolder = Common.CurrentAssemblyDirectory;

            //get settings
            var settingsFile = new FileInfo(Path.Combine(binFolder, "web.config"));
            Settings = new RebelCmsSettings(settingsFile);

            
            //FrameworkContext.Stub(x => x.CurrentLanguage).Return((LanguageInfo) Thread.CurrentThread.CurrentCulture);
            //FrameworkContext.Stub(x => x.TextManager).Return(MockRepository.GenerateMock<TextManager>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hive"></param>
        /// <param name="settings"></param>
        /// <param name="frameworkContext"></param>
        public FakeRebelCmsApplicationContext(IHiveManager hive, RebelCmsSettings settings, IFrameworkContext frameworkContext)
            : this(hive)
        {
            Hive = hive;
            Settings = settings;

            FrameworkContext = frameworkContext;
        }
        

        /// <summary>
        /// Puts an entity in to the repo
        /// </summary>
        /// <param name="e"></param>
        public void AddPersistenceData(TypedEntity e)
        {
            using (var unit = Hive.OpenWriter<IContentStore>())
            {
                unit.Repositories.AddOrUpdate(e);
                unit.Complete();
            }
        }

        public void AddPersistenceData(AbstractSchemaPart e)
        {
            using (var unit = Hive.OpenWriter<IContentStore>())
            {
                unit.Repositories.Schemas.AddOrUpdate(e);
                unit.Complete();
            }
        }

        /// <summary>
        /// Puts an entity in to the repo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        public void AddPersistenceData<T>(Revision<T> e)
            where T : TypedEntity
        {
            using (var unit = Hive.OpenWriter<IContentStore>())
            {
                unit.Repositories.Revisions.AddOrUpdate(e);
                unit.Complete();
            }
        }

        public IEnumerable<InstallStatus> GetInstallStatus()
        {
            //throw new NotImplementedException();
            return new[] {new InstallStatus(InstallStatusType.Completed)};
        }

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <remarks></remarks>
        public IFrameworkContext FrameworkContext { get; private set; }

        /// <summary>
        /// Gets the application id, useful for debugging or tracing.
        /// </summary>
        /// <value>The request id.</value>
        public Guid ApplicationId { get; private set; }

        /// <summary>
        /// Gets an instance of <see cref="HiveManager"/> for this application.
        /// </summary>
        /// <value>The hive.</value>
        public IHiveManager Hive { get; private set; }

        /// <summary>
        /// Gets the settings associated with this RebelCms application.
        /// </summary>
        /// <value>The settings.</value>
        public RebelCmsSettings Settings { get; private set; }

        /// <summary>
        /// Gets the security service.
        /// </summary>
        public ISecurityService Security { get; private set; }

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            Security.IfNotNull(x => x.DisposeIfDisposable());
            Settings.IfNotNull(x => x.DisposeIfDisposable());
            Hive.IfNotNull(x => x.DisposeIfDisposable());
            FrameworkContext.IfNotNull(x => x.Dispose());
        }

        #endregion
    }
}
