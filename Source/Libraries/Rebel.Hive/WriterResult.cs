using System;
using System.Collections.Generic;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Hive
{
    public class WriterResult<T> 
        where T : IProviderTypeFilter
    {
        public WriterResult(bool wasCommitted, bool wasRolledBack, params Exception[] errors)
        {
            WasCommitted = wasCommitted;
            WasRolledBack = wasRolledBack;
            Errors = errors;
        }

        public bool WasCommitted { get; private set; }
        public bool WasRolledBack { get; private set; }
        public IEnumerable<Exception> Errors { get; private set; }
    }
}