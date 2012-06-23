using System;
using System.Collections.Generic;
using System.Linq;
using RebelCms.Framework.DataManagement;
using RebelCms.Framework.Persistence.Abstractions;
using RebelCms.Framework.Persistence.DataManagement;
using RebelCms.Framework.Persistence.ProviderSupport;

namespace RebelCms.Framework.Persistence.MockedInMemory
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
