using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;


namespace Rebel.Framework.Persistence.Model
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a simple 1-dimensional keyed collection of entities
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    [DataContract(IsReference = true)]
    public class EntityCollection<T> : KeyedCollection<HiveId?, T>, INotifyCollectionChanged
        where T : AbstractEntity, IReferenceByAlias
    {
        public Action OnAdd;
        public Action<T> OnRemove;
        private readonly ReaderWriterLockSlim _addLocker = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim _removeLocker = new ReaderWriterLockSlim();

        public EntityCollection()
        {
        }

        public EntityCollection(IEnumerable<T> collection)
        {
            AddRange(collection);
        }

        public T this[string alias]
        {
            get { return this.FirstOrDefault(x => x.Alias.InvariantEquals(alias)); }
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null) return;
            collection.WhereNotNull().ForEach(Add);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection));
        }

        protected override void InsertItem(int index, T item)
        {
            using (new WriteLockDisposable(_addLocker))
            {
                var key = GetKeyForItem(item);
                if (key != null)
                {
                    var exists = this.Contains(key);
                    if (exists)
                    {
                        SetItem(IndexOfKey(key.Value), item);
                        return;
                    }
                }
                base.InsertItem(index, item);
                OnAdd.IfNotNull(x => x.Invoke());

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            }
            //base.InsertItem(index, item);
        }

        public new void Remove(HiveId id)
        {
            using (new WriteLockDisposable(_removeLocker))
            {
                var exists = Contains(id);
                if (!exists) return;
                var item = this[id];
                if (base.Remove(id))
                {
                    OnRemove.IfNotNull(x => x.Invoke(item));
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                }
            }
        }

        public new void Remove(T item)
        {
            using (new WriteLockDisposable(_removeLocker))
            {
                if (base.Remove(item))
                {
                    OnRemove.IfNotNull(x => x.Invoke(item));
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                }
            }
        }

        public int IndexOfKey(HiveId key)
        {
            for (var i = 0; i < this.Count; i++)
            {
                if (this[i].Id == key)
                {
                    return i;
                }
            }
            return -1;
        }

        #region Overrides of KeyedCollection<HiveId,T>

        /// <summary>
        /// When implemented in a derived class, extracts the key from the specified element.
        /// </summary>
        /// <returns>
        /// The key for the specified element.
        /// </returns>
        /// <param name="item">The element from which to extract the key.</param>
        protected override HiveId? GetKeyForItem(T item)
        {
            return item.Id.NullIfEmpty();
        }

        #endregion

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, args);
            }
        }
    }
}