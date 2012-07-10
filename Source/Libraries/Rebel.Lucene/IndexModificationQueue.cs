namespace Rebel.Lucene
{
    using System.Collections.Concurrent;

    /// <summary>
    /// Represents a queue of batches of work to be done to an index
    /// </summary>
    public class IndexModificationQueue
    {
        private readonly ConcurrentQueue<IndexModificationBatch> _batches = new ConcurrentQueue<IndexModificationBatch>();

        public ConcurrentQueue<IndexModificationBatch> Batches
        {
            get { return _batches; }
        }
    }
}