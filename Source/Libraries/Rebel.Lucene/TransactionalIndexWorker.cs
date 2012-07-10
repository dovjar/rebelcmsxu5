namespace Rebel.Lucene
{
    using System;
    using Rebel.Framework;
    using Rebel.Framework.Diagnostics;
    using global::Lucene.Net.Documents;
    using global::Lucene.Net.Index;

    public class TransactionalIndexWorker : DisposableObject
    {
        private readonly IndexWriter _indexWriter;
        private readonly IndexConfiguration _indexConfiguration;

        public TransactionalIndexWorker(IndexConfiguration indexConfiguration, IndexWriter indexWriter)
        {
            this._indexConfiguration = indexConfiguration;
            this._indexWriter = indexWriter;
        }

        public void Add(Document document)
        {
            _indexWriter.AddDocument(document);
        }

        public void RemoveAll()
        {
            _indexWriter.DeleteAll();
        }

        public void RemoveWhere(Term term)
        {
            _indexWriter.DeleteDocuments(term);
        }

        protected override void DisposeResources()
        {
            if (_indexWriter != null)
            {
                if (!IsCommitted)
                {
                    _indexWriter.Rollback();
                }
                _indexWriter.Close();
                _indexWriter.Dispose();
            }
        }

        public bool TryRollback()
        {
            if (IsCommitted) return false;
            try
            {
                _indexWriter.Rollback();
                IsCommitted = false;
            }
            catch (Exception ex)
            {
                LogHelper.Error<TransactionalIndexWorker>("Failed to rollback index writer: " + ex.Message, ex);
                return false;
            }
            return true;
        }

        public bool IsCommitted { get; protected set; }

        public bool TryOptimize()
        {
            try
            {
                _indexWriter.Optimize();
                return true;
            }
            catch(Exception ex)
            {
                /* Not interested */
                LogHelper.Warn<TransactionalIndexWorker>("Couldn't optimize the index, {0}", ex.Message);
            }
            return false;
        }

        public bool TryOptimizeDeletions()
        {
            try
            {
                _indexWriter.ExpungeDeletes();
                return true;
            }
            catch (Exception ex)
            {
                /* Not interested */
                LogHelper.Warn<TransactionalIndexWorker>("Couldn't clear unused deletion data from the index, {0}", ex.Message);
            }
            return false;
        }

        public void Commit()
        {
            try
            {
                //_indexWriter.Optimize();
                _indexWriter.WaitForMerges();
                _indexWriter.Commit();
                IsCommitted = true;
            }
            catch (Exception)
            {
                IsCommitted = false;
                throw;
            }
        }
    }
}