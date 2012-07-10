using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Framework.DataManagement.Linq;

namespace Rebel.Framework.Persistence.XmlStore.DataManagement.Linq
{
    public class XDocumentQueryContext<T> : AbstractQueryContext<T>
    {
        public XDocumentQueryContext(IQueryableDataSource queryableDataSource) : base(queryableDataSource)
        {
        }
    }
}
