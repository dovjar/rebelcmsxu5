namespace Rebel.Lucene.Caching
{
    #region Imports

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using Rebel.Framework;
    using Rebel.Framework.Caching;
    using Rebel.Framework.Context;
    using Rebel.Framework.Diagnostics;
    using Rebel.Framework.Serialization;
    using global::Lucene.Net.Documents;
    using global::Lucene.Net.Index;
    using global::Lucene.Net.Search;

    #endregion

    [DemandsDependencies(typeof(DemandBuilder))]
    public class CacheProvider : AbstractCacheProvider
    {
        private IndexController _indexController;
        private RuntimeCacheProvider _innerCacheProvider;
        private JsonNetSerializer _fixedSerializer = new JsonNetSerializer();
        private readonly ConcurrentHashedCollection<string> _removalCache = new ConcurrentHashedCollection<string>();
        private readonly bool _useInMemoryCache;

        public CacheProvider(IndexController indexController)
            : this(indexController, false)
        {}

        public CacheProvider(IndexController indexController, bool useInMemoryCache)
        {
            _useInMemoryCache = useInMemoryCache;
            _indexController = indexController;
            _indexController.Initialise();
            if (useInMemoryCache) _innerCacheProvider = new RuntimeCacheProvider();
        }

        protected AbstractSerializationService SerializationService
        {
            get { return _indexController.FrameworkContext.Serialization; }
        }

        protected override void DisposeResources()
        {
            _indexController.IfNotNull(x => x.Dispose());
        }

        public override CacheModificationResult AddOrChange<T>(string key, CacheValueOf<T> cacheObject)
        {
            if (_useInMemoryCache) _innerCacheProvider.AddOrChange(key, cacheObject);

            var existing = GetCacheEntry(key, true);

            try
            {
                if (existing != null)
                {
                    _indexController.QueueRemoveWhere(new Term("CacheKey", key));
                }

                var doc = new Document();

                ModifyDocument(doc, key, cacheObject);

                LogHelper.TraceIfEnabled<CacheProvider>("Queueing an item to the index");
                _indexController.QueueAdd(doc);

                return new CacheModificationResult(existing != null, existing == null);
            }
            finally
            {
                _removalCache.Remove(key);
            }
        }

        private bool ModifyDocument<T>(Document doc, string key, CacheValueOf<T> cacheObject)
        {
            var cachePolicy = cacheObject.Policy;

            // TODO: Disabled for beta due to a bug in ServiceStack deserializing CompositeEntitySchema: var totalJson = SerializationService.ToJson(cacheObject);
            var jsonStream = _fixedSerializer.ToStream(cacheObject);
            if (!jsonStream.Success) return false;

            var totalJson = jsonStream.ResultStream.ToJsonString();

            doc.Add(new Field("CacheKey", key, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("CacheEntry", totalJson, Field.Store.YES, Field.Index.NOT_ANALYZED));

            doc.Add(new NumericField("ExpiryTicks", Field.Store.YES, false).SetLongValue(cachePolicy.ExpiryDate.Ticks));

            var utcNow = DateTimeOffset.Now;
            doc.Add(new NumericField("Created", Field.Store.YES, false).SetLongValue(utcNow.Ticks));

            return true;
        }

        public override void Clear()
        {
            if (_useInMemoryCache) _innerCacheProvider.IfNotNull(x => x.Clear());
            _indexController.QueueRemoveAll();
            _indexController.ForceFlushQueue();
        }

        public override bool Remove(string key)
        {
            if (_useInMemoryCache) _innerCacheProvider.Remove(key);
            var existed = GetCacheEntry(key);

            if (existed != null)
            {
                _removalCache.Add(key);
                _indexController.QueueRemoveWhere(new Term("CacheKey", key));
                //Don't need to force the queue to flush because we have _removalCache to prevent
                //us from returning something anyway, so we can wait for the queue fine _indexController.ForceFlushQueue();
            }
            return existed != null;
        }

        protected override IEnumerable<string> GetKeysMatching<T>(Func<T, bool> predicate)
        {
            var allMatches = new List<string>();
            var allTerms = new List<string>();

            var scopedReader = _indexController.GetScopedReader();

            var cachekey = "CacheKey";
            var termScanner = scopedReader.InnerReader.Terms(new Term(cachekey, string.Empty));

            // Lucene doesn't start at -1 so can't just do "while termScanner.Next()"
            var termEnumerator = termScanner.Term();
            while (termEnumerator != null && termEnumerator.Field() == "CacheKey")
            {
                var asText = termEnumerator.Text();
                allTerms.Add(asText);

                var asKey = (CacheKey<T>)asText;
                if (asKey != null)
                {
                    var matches = predicate.Invoke(asKey.Original);
                    if (matches)
                        allMatches.Add(asText);
                }
                termEnumerator = termScanner.Next() ? termScanner.Term() : null;
            }
            return allMatches;
        }

        protected override IEnumerable<string> GetKeysMatching(string containing)
        {
            var allMatches = new List<string>();
            var allTerms = new List<string>();

            var scopedReader = _indexController.GetScopedReader();

            var cachekey = "CacheKey";
            var termScanner = scopedReader.InnerReader.Terms(new Term(cachekey, string.Empty));

            // Lucene doesn't start at -1 so can't just do "while termScanner.Next()"
            var termEnumerator = termScanner.Term();
            while (termEnumerator != null && termEnumerator.Field() == "CacheKey")
            {
                var asText = termEnumerator.Text();
                allTerms.Add(asText);

                if(asText.Contains(containing))
                    allMatches.Add(asText);

                termEnumerator = termScanner.Next() ? termScanner.Term() : null;
            }
            return allMatches;
        }

        private Document GetCacheEntry(string key, bool ignoreRemovalQueue = false)
        {
            if (!ignoreRemovalQueue && _removalCache.Contains(key)) return null;

            var scopedSearcher = _indexController.GetScopedLuceneSearcher();

            var query = new BooleanQuery();
            query.Add(new BooleanClause(new TermQuery(new Term("CacheKey", key)), BooleanClause.Occur.MUST));
            var topDocs = scopedSearcher.Search(query, 1);
            var topDoc = topDocs.ScoreDocs.Any() ? topDocs.ScoreDocs[0] : null;

            if (topDoc != null)
            {
                return scopedSearcher.Doc(topDoc.doc);
            }
            return null;
        }

        protected override CacheEntry<T> PerformGet<T>(string key)
        {
            var memEntry = _useInMemoryCache ? _innerCacheProvider.Get<T>(key) : null;
            if (memEntry != null) return new CacheEntry<T>(key, memEntry);

            var entry = GetCacheEntry(key);

            if (entry != null)
            {
                var cacheEntryAsString = entry.Get("CacheEntry");
                try
                {
                    // TODO: Disabled for beta due to a bug in ServiceStack deserializing CompositeEntitySchema: var cacheEntry = SerializationService.FromJson<CacheValueOf<T>>(cacheEntryAsString);
                    var cacheEntry = _fixedSerializer.FromJson<CacheValueOf<T>>(cacheEntryAsString);

                    var entryFromIndex = ReferenceEquals(cacheEntry, null)
                        ? null
                        : new CacheEntry<T>(key, cacheEntry);

                    if (entryFromIndex != null)
                    {
                        // Ensure we put it in our in-memory cache too
                        if (_useInMemoryCache) _innerCacheProvider.AddOrChange(key, cacheEntry);
                        return entryFromIndex;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Warn<CacheProvider>("Deserializing an entry from cache failed, {0}", ex.Message);
                }
            }

            return null;
        }
    }
}