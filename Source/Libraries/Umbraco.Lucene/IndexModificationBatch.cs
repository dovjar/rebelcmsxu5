namespace Umbraco.Lucene
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Represents a queue of work to be done to an index.
    /// </summary>
    public class IndexModificationBatch
    {
        private readonly IProducerConsumerCollection<Action<TransactionalIndexWorker>> _batch = new ConcurrentQueue<Action<TransactionalIndexWorker>>();

        public IndexModificationBatch(params Action<TransactionalIndexWorker>[] work)
        {
            _batch = new ConcurrentQueue<Action<TransactionalIndexWorker>>(work);
        }

        public IProducerConsumerCollection<Action<TransactionalIndexWorker>> Batch
        {
            get { return _batch; }
        }

        public bool Enqueue(Action<TransactionalIndexWorker> work)
        {
            return _batch.TryAdd(work);
        }
    }
}