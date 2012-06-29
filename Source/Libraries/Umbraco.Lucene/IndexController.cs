namespace Umbraco.Lucene
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Umbraco.Framework;
    using Umbraco.Framework.Context;
    using Umbraco.Framework.Diagnostics;
    using global::Lucene.Net.Analysis.Standard;
    using global::Lucene.Net.Documents;
    using global::Lucene.Net.Index;
    using global::Lucene.Net.Search;
    using global::Lucene.Net.Store;
    using Directory = global::Lucene.Net.Store.Directory;
    using Version = global::Lucene.Net.Util.Version;

    /// <summary>
    /// A simple index controller centred around asynchronous queuing of index modifications.
    /// </summary>
    public class IndexController : DisposableObject, IRequiresFrameworkContext
    {
        private static ReaderWriterLockSlim _directoryCheckLocker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private Func<IFrameworkContext> _frameworkContextGetter;
        public IndexController(IndexConfiguration configuration, Func<IFrameworkContext> frameworkContextGetter)
            : this(configuration, (IFrameworkContext)null)
        {
            _frameworkContextGetter = frameworkContextGetter;
        }

        public IndexController(IndexConfiguration configuration, IFrameworkContext frameworkContext)
        {
            Configuration = configuration;
            FrameworkContext = frameworkContext;
            _manualResetter = new ManualResetEvent(false);
            Queue = new IndexModificationQueue();
        }

        /// <summary>
        /// Sets the framework context. Used within IoC registration to avoid a circular reference / stack overflow because IFrameworkContext has Caches which could have this provider, which needs an IFrameworkContext
        /// </summary>
        /// <param name="frameworkContext">The framework context.</param>
        internal void SetFrameworkContext(IFrameworkContext frameworkContext)
        {
            FrameworkContext = frameworkContext;
        }

        public IndexModificationQueue Queue { get; protected set; }

        /// <summary>
        /// Queues a batch of index modifications.
        /// </summary>
        /// <param name="work">The work.</param>
        public void QueueBatch(params Action<TransactionalIndexWorker>[] work)
        {
            Queue.Batches.Enqueue(new IndexModificationBatch(work));
        }

        /// <summary>
        /// Queues the addition of an index document.
        /// </summary>
        /// <param name="document">The document.</param>
        public void QueueAdd(Document document)
        {
            Queue.Batches.Enqueue(new IndexModificationBatch(x => x.Add(document)));
        }

        /// <summary>
        /// Queues the removal of all index documents.
        /// </summary>
        /// <remarks>Caller should also call <see cref="ForceFlushQueue"/> if they wish to block until the removal is flushed.</remarks>
        public void QueueRemoveAll()
        {
            Queue.Batches.Enqueue(new IndexModificationBatch(x => x.RemoveAll()));
        }

        /// <summary>
        /// Queues the removal of an index document matching the supplied <paramref name="term"/>.
        /// </summary>
        /// <param name="term">The term.</param>
        public void QueueRemoveWhere(Term term)
        {
            Queue.Batches.Enqueue(new IndexModificationBatch(x => x.RemoveWhere(term)));
        }

        private static ReaderWriterLockSlim _flushLocker = new ReaderWriterLockSlim();
        public void PartialFlushQueue(int flushLimit)
        {
            if (_forceStop) return;

            using (new WriteLockDisposable(_flushLocker))
            {
                if (_forceStop || !Queue.Batches.Any()) return;
                using (var worker = CreateTransactionWorker())
                {
                    try
                    {
                        for (int i = 0; i < flushLimit; i++)
                        {
                            if (_forceStop) break;

                            IndexModificationBatch batch = null;
                            var success = Queue.Batches.TryDequeue(out batch);
                            if (success)
                            {
                                EnactBatch(worker, batch);
                                _flushCount++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        worker.Commit();
                        if (_flushCount % 50 == 0) worker.TryOptimizeDeletions();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<IndexController>("Failed to partially flush the queue: " + ex.Message, ex);
                        worker.TryRollback();
                        throw;
                    }
                }
            }
        }

        public void ForceFlushQueue()
        {
            LogHelper.TraceIfEnabled<IndexController>("ForceFlushQueue called");
            FlushQueue();
        }

        private int _flushCount = 0;
        protected void FlushQueue()
        {
            if (_forceStop) return;
            using (new WriteLockDisposable(_flushLocker))
            {
                if (_forceStop) return;

                LogHelper.TraceIfEnabled<IndexController>("Checking for items in FlushQueue");
                IndexModificationBatch batch = null;
                var success = Queue.Batches.TryDequeue(out batch);

                if (!success) return;

                using (DisposableTimer.TraceDuration<IndexController>("Creating a worker and flushing", "Worker for FlushQueue finished"))
                using (var worker = CreateTransactionWorker())
                {
                    try
                    {
                        while (success && !_forceStop)
                        {
                            EnactBatch(worker, batch);
                            _flushCount++;
                            success = Queue.Batches.TryDequeue(out batch);
                        }
                        worker.Commit();
                        if (_flushCount % 50 == 0) worker.TryOptimizeDeletions();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<IndexController>("Failed to flush the queue: " + ex.Message, ex);
                        worker.TryRollback();
                        throw;
                    }
                }
            }
        }

        private static void EnactBatch(TransactionalIndexWorker worker, IndexModificationBatch batch)
        {
            if (batch == null) return;
            try
            {
                foreach (var action in batch.Batch)
                {
                    action.Invoke(worker);
                }
            }
            catch (Exception innerEx)
            {
                LogHelper.Error<IndexController>("Failed to perform a batch operation: " + innerEx.Message, innerEx);
                throw;
            }
        }

        private bool _isInitialised = false;
        public void Initialise()
        {
            if (IsInitialised) return;

            using (new WriteLockDisposable(_directoryCheckLocker))
            {
                var directory = GetBuildLocationInfo();
                if (!directory.Exists || !directory.EnumerateFiles().Any())
                    using (new WriteLockDisposable(_directoryCheckLocker))
                    {
                        if (!directory.Exists || !directory.EnumerateFiles().Any())
                        {
                            directory.Create();
                            InitialiseBlankIndex(GetLuceneDirectory(directory));
                        }
                    }
            }

            StartQueueMonitor();
        }

        private bool _shuttingDown = false;

        /// <summary>
        /// Starts the queue monitor <see cref="Task"/>.
        /// </summary>
        private void StartQueueMonitor()
        {
            if (_shuttingDown) return;

            LogHelper.TraceIfEnabled<IndexController>("Starting queue monitor");
            var queueMonitorTask = new Task(MonitorIndexBatch, TaskCreationOptions.LongRunning);
            // Using a recursive ContinueWith ensures the task thread will restart after logging any errors that caused it to fail
            queueMonitorTask.LogErrors(LogQueueMonitorError).ContinueWith(x => StartQueueMonitor()).LogErrors(LogQueueMonitorError);
            queueMonitorTask.Start();
        }

        private static void LogQueueMonitorError(string logMessage, Exception exception)
        {
            LogHelper.Error<IndexController>(logMessage, exception);
        }

        private bool _hasFinished = false;
        private readonly ManualResetEvent _manualResetter;

        /// <summary>
        /// Repeatedly checks <see cref="IndexModificationQueue"/> for new batches, and calls <see cref="FlushQueue"/> if any are found.
        /// </summary>
        private void MonitorIndexBatch()
        {
            try
            {
                LogHelper.TraceIfEnabled<IndexController>("In queue monitor");
                while (!_shuttingDown)
                {
                    if (_shuttingDown) break;
                    while (!Queue.Batches.Any())
                    {
                        Thread.Sleep(250);
                        if (_shuttingDown) return; // Will invoke finally block, don't forget
                    }
                    LogHelper.TraceIfEnabled<IndexController>("Calling FlushQueue from monitor");
                    FlushQueue();
                }
            }
            finally
            {
                _hasFinished = true;
                _manualResetter.Set();
            }
        }

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public IndexConfiguration Configuration { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this instance is initialised.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is initialised; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitialised
        {
            get { return _isInitialised; }
        }

        internal IndexWriter CreateLuceneWriter()
        {
            var directory = GetLuceneDirectory();
            var standardAnalyzer = GetAnalyzer();
            return CreateLuceneWriter(standardAnalyzer, directory);
        }

        internal static StandardAnalyzer GetAnalyzer()
        {
            return new StandardAnalyzer(Version.LUCENE_29);
        }

        internal Directory GetLuceneDirectory()
        {
            return GetLuceneDirectory(GetBuildLocationInfo());
        }

        internal Directory GetLuceneDirectory(DirectoryInfo directory)
        {
            return FSDirectory.Open(directory);
        }

        private DirectoryInfo GetBuildLocationInfo()
        {
            return new DirectoryInfo(Configuration.BuildLocation);
        }

        internal static void InitialiseBlankIndex(Directory directory)
        {
            new IndexWriter(directory,
                            GetAnalyzer(),
                            true,
                            IndexWriter.MaxFieldLength.UNLIMITED).Dispose();
        }

        internal static IndexWriter CreateLuceneWriter(StandardAnalyzer standardAnalyzer, Directory directory)
        {
            return new IndexWriter(directory,
                                   standardAnalyzer,
                                   false,
                                   IndexWriter.MaxFieldLength.UNLIMITED);
        }

        internal static global::Lucene.Net.Index.IndexReader CreateLuceneReader(Directory directory, bool secondAttempt = false)
        {
            try
            {
                // Scan the index directory for segments files to check for corruption
                var files = directory.ListAll();
                var foundSegments = false;
                foreach (var file in files)
                {
                    if (file.StartsWith("segments", StringComparison.InvariantCultureIgnoreCase))
                    {
                        foundSegments = true;
                        break;
                    }
                }
                if (!foundSegments)
                {
                    foreach (var file in files)
                    {
                        directory.DeleteFile(file);
                    }
                }

                var reader = global::Lucene.Net.Index.IndexReader.Open(directory, true);
                return reader;
            }
            catch (FileNotFoundException fnf)
            {
                InitialiseBlankIndex(directory);
                if (!secondAttempt) return CreateLuceneReader(directory, true);
                throw new InvalidOperationException("Could not create an index reader - tried twice", fnf);
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException("Could not create an index reader", ex);
            }
        }

        internal global::Lucene.Net.Index.IndexReader CreateLuceneReader()
        {
            var directory = GetLuceneDirectory();
            return CreateLuceneReader(directory);
        }

        private bool _forceStop = false;
        protected override void DisposeResources()
        {
            LogHelper.Warn<IndexController>("Disposing - waiting 5s for indexing to finish");
            _shuttingDown = true;
            _manualResetter.WaitOne(TimeSpan.FromSeconds(5));

            if (!_hasFinished)
            {
                LogHelper.Warn<IndexController>("Having to force-stop an open index batch because it took longer than 5s to finish");
                _forceStop = true;
            }
        }

        private TransactionalIndexWorker CreateTransactionWorker()
        {
            return new TransactionalIndexWorker(Configuration, CreateLuceneWriter());
        }

        /// <summary>
        /// Gets an index searcher by using the reader whose lifetime is managed by <see cref="GetScopedReader"/>.
        /// </summary>
        /// <returns></returns>
        public IndexSearcher GetScopedLuceneSearcher()
        {
            var reader = GetScopedReader();
            return reader.NewSearcher();
        }

        /// <summary>
        /// Gets a reader that is added to <see cref="IFrameworkContext.ScopedCache"/>, or returns the one already added. Disposal is handled by <see cref="IFrameworkContext.ScopedFinalizer"/>.
        /// </summary>
        /// <returns></returns>
        public IndexReader GetScopedReader()
        {
            var toReturn = FrameworkContext
                .ScopedCache
                .GetOrCreateTyped("index-cache-reader",
                                  () =>
                                  {
                                      var reader = CreateDisposableReader();
                                      FrameworkContext.ScopedFinalizer.AddFinalizerToScope(reader, x => x.Dispose());
                                      return reader;
                                  });
            if (!toReturn.InnerReader.IsCurrent())
                toReturn.InnerReader = toReturn.InnerReader.Reopen();
            return toReturn;
        }

        /// <summary>
        /// Creates an index reader. The caller is responsible for disposing the reader when done.
        /// </summary>
        /// <returns></returns>
        public IndexReader CreateDisposableReader()
        {
            return new IndexReader(Configuration, CreateLuceneReader());
        }

        private IFrameworkContext _frameworkContext;

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <remarks></remarks>
        public IFrameworkContext FrameworkContext
        {
            get { return _frameworkContext ?? (_frameworkContext = _frameworkContextGetter.Invoke()); }
            protected set { _frameworkContext = value; }
        }
    }
}