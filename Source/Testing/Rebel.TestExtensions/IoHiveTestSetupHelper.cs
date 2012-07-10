using System;
using System.IO;
using System.Reflection;
using System.Text;
using Rebel.Framework.Context;
using Rebel.Framework.Persistence.ProviderSupport._Revised;
using Rebel.Hive;
using Rebel.Hive.Configuration;
using Rebel.Hive.ProviderSupport;
using Rebel.Hive.Providers.IO;
using Rebel.Tests.Extensions.Stubs;
using File = Rebel.Framework.Persistence.Model.IO.File;

namespace Rebel.Tests.Extensions
{
    public class IoHiveTestSetupHelper
    {
        public IoHiveTestSetupHelper(FakeFrameworkContext frameworkContext = null)
            : this("*.dll", frameworkContext)
        {
                
        }

        public IoHiveTestSetupHelper(string supportedExtensions, FakeFrameworkContext frameworkContext = null)
        {
            FrameworkContext = frameworkContext ?? new FakeFrameworkContext();
            TestDirectory = GetTestDirectory();
            ProviderMetadata = new ProviderMetadata("test-provider", new Uri("storage://"), true, false);
            Settings = new Settings(supportedExtensions, TestDirectory.FullName, TestDirectory.FullName, String.Empty, String.Empty, "~/");
            EntityRepositoryFactory = new EntityRepositoryFactory(ProviderMetadata, null, null, Settings, FrameworkContext);
            EntityRepository = EntityRepositoryFactory.GetRepository() as EntityRepository;

            var readonlyFactory = new ReadonlyProviderUnitFactory(EntityRepositoryFactory);
            var factory = new ProviderUnitFactory(EntityRepositoryFactory);

            ReadonlyProviderSetup = new ReadonlyProviderSetup(readonlyFactory, ProviderMetadata, FrameworkContext, new NoopProviderBootstrapper(), 0);
            ProviderSetup = new ProviderSetup(factory, ProviderMetadata, FrameworkContext, new NoopProviderBootstrapper(), 0);
        }

        public IFrameworkContext FrameworkContext { get; set; }
        public DirectoryInfo TestDirectory { get; set; }
        public ProviderMetadata ProviderMetadata { get; set; }
        public Settings Settings { get; set; }
        public EntityRepositoryFactory EntityRepositoryFactory { get; set; }
        public EntityRepository EntityRepository { get; set; }

        public ProviderSetup ProviderSetup { get; set; }
        public ReadonlyProviderSetup ReadonlyProviderSetup { get; set; }

        public ProviderMappingGroup CreateGroup(string name, string route)
        {
            return new ProviderMappingGroup(name, new WildcardUriMatch(route), ReadonlyProviderSetup, ProviderSetup, FrameworkContext);
        }

        public static DirectoryInfo GetTestDirectory()
        {
            var dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            return new DirectoryInfo(Path.Combine(dir.FullName, @"..\..\"));
        }

        public static File CreateFile(string contents)
        {
            var file = new File
                {
                    Name = Guid.NewGuid().ToString("N"),
                };

            if (!String.IsNullOrEmpty(contents))
                file.ContentBytes = Encoding.Default.GetBytes(contents);
            else
                file.IsContainer = true;

            return file;
        }
    }
}