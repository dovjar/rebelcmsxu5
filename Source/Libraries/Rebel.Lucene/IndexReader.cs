namespace Rebel.Lucene
{
    using System.Linq;
    using Rebel.Framework;
    using global::Lucene.Net.Documents;
    using global::Lucene.Net.Index;
    using global::Lucene.Net.Search;

    public class IndexReader : DisposableObject
    {
        private global::Lucene.Net.Index.IndexReader _indexReader;
        private readonly IndexConfiguration _indexConfiguration;

        public IndexReader(IndexConfiguration indexConfiguration, global::Lucene.Net.Index.IndexReader indexReader)
        {
            _indexReader = indexReader;
            _indexConfiguration = indexConfiguration;
        }

        public global::Lucene.Net.Index.IndexReader InnerReader
        {
            get { return _indexReader; }
            set
            {
                _indexReader.IfNotNull(x =>
                {
                    if (!ReferenceEquals(x, value))
                        x.Dispose();
                });
                _indexReader = value;
            }
        }

        protected override void DisposeResources()
        {
            InnerReader.IfNotNull(x => x.Dispose());
        }

        public IndexSearcher NewSearcher()
        {
            return new IndexSearcher(InnerReader);
        }
    }
}