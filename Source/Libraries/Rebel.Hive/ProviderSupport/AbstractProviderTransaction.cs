using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Framework;

namespace Rebel.Hive.ProviderSupport
{
    using Rebel.Framework.Diagnostics;

    public abstract class AbstractProviderTransaction : DisposableObject, IProviderTransaction
    {
        protected abstract bool PerformExplicitRollback();
        protected abstract bool PerformImplicitRollback();
        protected abstract bool PerformCommit();
        public abstract bool IsTransactional { get; }

        private readonly List<Action> _cacheFlushActions = new List<Action>();

        protected AbstractProviderTransaction()
        {
            //when a transaction is created, it is effectively active
            IsActive = true;

            WasCommitted = false;
            WasRolledBack = false;
        }

        public virtual string GetTransactionId()
        {
            return GetHashCode().ToString();
        }

        public abstract void EnsureBegun();

        /// <summary>
        /// Determines whether this transaction has actions to perform prior to committing, e.g. cache flushing.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if has commital actions to perform; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool HasCommitalActionsToPerform()
        {
            return CacheFlushActions != null && CacheFlushActions.Any();
        }

        /// <summary>
        /// Performs the pre commital actions, e.g. cache flushing.
        /// </summary>
        public virtual void PerformPreCommitalActions()
        {
            CacheFlushActions.IfNotNull(x => x.ForEach(y => y.Invoke()));
        }

        #region Implementation of ITransaction

        /// <summary>Commits this transaction. </summary>
        public void Commit(IProviderUnit unit)
        {
            this.CheckThrowObjectDisposed(base.IsDisposed, this.GetType().Name + "...Transaction:Commit");

            if (!IsActive || WasRolledBack) return;

            try
            {
                PerformPreCommitalActions();
            }
            catch (Exception ex)
            {
                LogHelper.Error<AbstractProviderTransaction>("While completing a transaction, PerformPreCommitalActions threw an error. The transaction will continue.", ex);
            }
            WasCommitted = PerformCommit();
            if (WasCommitted)
                IsActive = false;
        }

        /// <summary>Rolls back this transaction. </summary>
        public void Rollback(IProviderUnit unit)
        {
            if (!IsActive || WasCommitted)
                return;

            this.CheckThrowObjectDisposed(base.IsDisposed, this.GetType().Name + "...Transaction:Rollback");
            WasRolledBack = PerformExplicitRollback();
            if (WasRolledBack)
                IsActive = false;
        }

        /// <summary>
        /// Rolls back this transaction and stores a value for whether the rollback request was as a result of an implicit operation, like the disposal of the transaction.
        /// </summary>
        /// <param name="requestIsImplicit">if set to <c>true</c> [request is implicit].</param>
        /// <remarks></remarks>
        public void Rollback(bool requestIsImplicit)
        {
            if (!IsActive || WasCommitted)
                return;

            if (!requestIsImplicit)
            {
                //if its explicit, the do a normal rollback
                Rollback(null);
            }
            else
            {
                this.CheckThrowObjectDisposed(base.IsDisposed, this.GetType().Name + "...Transaction:Rollback(true)");
                WasRolledBack = PerformImplicitRollback();
                if (WasRolledBack)
                    IsActive = false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        public virtual bool IsActive { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance was committed.
        /// </summary>
        /// <value><c>true</c> if committed; otherwise, <c>false</c>.</value>
        public virtual bool WasCommitted { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance was rolled back.
        /// </summary>
        /// <value><c>true</c> if rolled back; otherwise, <c>false</c>.</value>
        public virtual bool WasRolledBack { get; private set; }

        /// <summary>
        /// Gets a list of actions that will be performed on completion, for example cache updating
        /// </summary>
        /// <value>The on completion actions.</value>
        public List<Action> CacheFlushActions
        {
            get
            {
                return _cacheFlushActions;
            }
        }

        #endregion

        protected override void DisposeResources()
        {
            IsActive = false;
            return;
        }
    }
}
