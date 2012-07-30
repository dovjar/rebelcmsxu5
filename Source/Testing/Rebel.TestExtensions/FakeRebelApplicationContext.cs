using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NSubstitute;
using Rebel.Cms;
using Rebel.Cms.Web;
using Rebel.Cms.Web.Configuration;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Security;
using Rebel.Cms.Web.Security.Permissions;
using Rebel.Framework;
using Rebel.Framework.Context;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Framework.ProviderSupport;
using Rebel.Framework.Security;
using Rebel.Framework.Testing;
using Rebel.Hive;
using Rebel.Hive.RepositoryTypes;
using Rebel.Tests.Extensions.Stubs;

namespace Rebel.Tests.Extensions
{
    public class FakeRebelApplicationContext : DisposableObject, IRebelApplicationContext
    {
        //private readonly NHibernateInMemoryRepository _repo;

        public FakeRebelApplicationContext(bool addSystemRooNode = true)
            : this(FakeHiveCmsManager.New(new FakeFrameworkContext()), addSystemRooNode)
        {
            
        }

        public FakeRebelApplicationContext(IHiveManager hive, bool addSystemRooNode = true)
        {
            ApplicationId = Guid.NewGuid();

            //_repo = new NHibernateInMemoryRepository(cmsManager.CoreManager.FrameworkContext);

            Hive = hive;
            FrameworkContext = Hive.FrameworkContext;
            //Security = MockRepository.GenerateMock<ISecurityService>();
            Security = Substitute.For<ISecurityService>();
            Security.Permissions.GetEffectivePermission(Arg.Any<Guid>(), Arg.Any<HiveId>(), Arg.Any<HiveId>())
                .Returns(new PermissionResult(new BackOfficeAccessPermission(), HiveId.Empty, PermissionStatus.Allow));
            Security.Permissions.GetEffectivePermissions(Arg.Any<HiveId>(), Arg.Any<HiveId>(), Arg.Any<Guid[]>())
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
            Settings = new RebelSettings(settingsFile);

            
            //FrameworkContext.Stub(x => x.CurrentLanguage).Return((LanguageInfo) Thread.CurrentThread.CurrentCulture);
            //FrameworkContext.Stub(x => x.TextManager).Return(MockRepository.GenerateMock<TextManager>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hive"></param>
        /// <param name="settings"></param>
        /// <param name="frameworkContext"></param>
        public FakeRebelApplicationContext(IHiveManager hive, RebelSettings settings, IFrameworkContext frameworkContext)
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

        public bool IsFirstRun { get; set; }

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
        /// Gets the settings associated with this Rebel application.
        /// </summary>
        /// <value>The settings.</value>
        public RebelSettings Settings { get; private set; }

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
