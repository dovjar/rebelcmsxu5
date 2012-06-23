using System;

namespace RebelCms.Hive.ProviderSupport
{
    public class TransactionCompletedException : Exception
    {
        public TransactionCompletedException(string transactionIsNotActive)
            : base(transactionIsNotActive)
        { }
    }
}