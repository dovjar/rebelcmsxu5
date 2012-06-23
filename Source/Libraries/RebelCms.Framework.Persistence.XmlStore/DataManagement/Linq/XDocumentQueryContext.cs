using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCms.Framework.DataManagement.Linq;

namespace RebelCms.Framework.Persistence.XmlStore.DataManagement.Linq
{
    public class XDocumentQueryContext<T> : AbstractQueryContext<T>
    {
        public XDocumentQueryContext(IQueryableDataSource queryableDataSource) : base(queryableDataSource)
        {
        }
    }
}
