using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Security;
using Examine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis.Standard;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Framework;
using Rebel.Framework.Persistence.Examine;
using Rebel.Framework.Persistence.Examine.Hive;
using Rebel.Framework.Persistence.Examine.Mapping;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.ProviderSupport._Revised;
using Rebel.Framework.Testing;
using Rebel.Framework.TypeMapping;
using Rebel.Hive;
using Rebel.Hive.ProviderSupport;

namespace Rebel.Tests.Extensions
{
    using System.Threading;
    using Rebel.Framework.Diagnostics;

    public class ExamineTestSetupHelper : DisposableObject
    {
        
        public ExamineTestSetupHelper(FakeFrameworkContext frameworkContext = null, bool isPassthrough = false)
        {
            var examineWorkingFolder = new DirectoryInfo(Path.Combine(Common.CurrentAssemblyDirectory, "Examine", Guid.NewGuid().ToString() + Thread.CurrentThread.ManagedThreadId));
            if (!examineWorkingFolder.Exists)
                Directory.CreateDirectory(examineWorkingFolder.FullName);

            //clear out old folders
            var parentFolder = examineWorkingFolder.Parent;
            foreach(var f in parentFolder.GetDirectories().Where(x => x.CreationTimeUtc < DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(5))))
            {
                try
                {
                    Directory.Delete(f.FullName, true);
                }
                catch (IOException)
                {
                    //ignore
                }
            }

            LogHelper.TraceIfEnabled<ExamineTestSetupHelper>("Index setup in folder {0}", () => examineWorkingFolder.FullName);

            //var disk = new Lucene.Net.Store.RAMDirectory();
            var disk = new Lucene.Net.Store.SimpleFSDirectory(examineWorkingFolder);

            Indexer = new RebelExamineIndexer(
                new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29),
                SynchronizationType.Synchronized,
                disk);

            _fakeFrameworkContext = frameworkContext ?? new FakeFrameworkContext();
            

            Searcher = new LuceneSearcher(new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29), disk);
            ExamineManager = new ExamineManager(new[] { Searcher }, new[] { Indexer }, Searcher);
            ExamineHelper = new ExamineHelper(ExamineManager, _fakeFrameworkContext, false); //false to not use cache

            var examineMapper = new ExamineModelMapper(ExamineHelper, _fakeFrameworkContext);
            _fakeFrameworkContext.SetTypeMappers(new FakeTypeMapperCollection(new AbstractMappingEngine[] { examineMapper, new FrameworkModelMapper(_fakeFrameworkContext) }));

            var providerMetadata = new ProviderMetadata("r-examine-unit-tester", new Uri("tester://"), true, isPassthrough);
 
            var revisionSchemaSessionFactory = new NullProviderRevisionRepositoryFactory<EntitySchema>(providerMetadata, FrameworkContext);
            var revisionRepositoryFactory = new RevisionRepositoryFactory(providerMetadata, FrameworkContext, ExamineHelper);
            //var revisionRepositoryFactory = new NullProviderRevisionSessionFactory<TypedEntity>(providerMetadata, FrameworkContext);
            var schemaRepositoryFactory = new SchemaRepositoryFactory(providerMetadata, revisionSchemaSessionFactory, FrameworkContext, ExamineHelper);
            var entityRepositoryFactory = new EntityRepositoryFactory(providerMetadata, revisionRepositoryFactory, schemaRepositoryFactory, FrameworkContext, ExamineHelper);

            var readUnitFactory = new ReadonlyProviderUnitFactory(entityRepositoryFactory);
            var unitFactory = new ProviderUnitFactory(entityRepositoryFactory);

            ProviderSetup = new ProviderSetup(unitFactory, providerMetadata, FrameworkContext, null, 0);
            ReadonlyProviderSetup = new ReadonlyProviderSetup(readUnitFactory, providerMetadata, FrameworkContext, null, 0);

            //ensure that the index exists
            Indexer.CreateIndex(true);
        }

        private readonly FakeFrameworkContext _fakeFrameworkContext;

        public ProviderSetup ProviderSetup { get; private set; }
        public ReadonlyProviderSetup ReadonlyProviderSetup { get; private set; }

        public ExamineHelper ExamineHelper { get; private set; }
        public ExamineManager ExamineManager{ get; private set; }
        public LuceneSearcher Searcher { get; private set; }
        public LuceneIndexer Indexer { get; private set; }

        public FakeFrameworkContext FrameworkContext
        {
            get { return _fakeFrameworkContext; }
        }

        protected override void DisposeResources()
        {
            ExamineHelper.Dispose();
        }
    }
}