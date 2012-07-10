using System;

namespace Rebel.Hive.ProviderSupport
{
    public class TransactionCompletedException : Exception
    {
        public TransactionCompletedException(string transactionIsNotActive)
            : base(transactionIsNotActive)
        { }
    }
}