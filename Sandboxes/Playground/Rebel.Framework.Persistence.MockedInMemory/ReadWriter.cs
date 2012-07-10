using System;
using System.Collections.Generic;
using System.Linq;
using Rebel.Framework.DataManagement;
using Rebel.Framework.Persistence.Abstractions;
using Rebel.Framework.Persistence.DataManagement;
using Rebel.Framework.Persistence.ProviderSupport;

namespace Rebel.Framework.Persistence.MockedInMemory
{
    public class ReadWriter : ReadWriterBase
    {
        private ReadWriter()
            : base()
        {
        }

        public ReadWriter(IReadWriteRepositoryUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }
    }
}
