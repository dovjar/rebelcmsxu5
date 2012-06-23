using System;
using System.Xml.Linq;

using RebelCms.Framework.Context;
using RebelCms.Framework.DataManagement;
using RebelCms.Framework.Persistence.DataManagement;
using RebelCms.Framework.Persistence.ProviderSupport;
using RebelCms.Framework.Persistence.XmlStore.DataManagement.Linq;

namespace RebelCms.Framework.Persistence.XmlStore.DataManagement
{
    public class DataContextFactory : AbstractDataContextFactory
    {
        private readonly string _xmlPath;
        private readonly Guid _contextId;
        public DataContextFactory(string xmlPath)
        {
            _xmlPath = xmlPath;
            _contextId = Guid.NewGuid();
        }

        protected override AbstractDataContext InstantiateDataContext(IHiveProvider hiveProvider)
        {
            return new DataContext(hiveProvider, XDocument.Load(_xmlPath), _xmlPath, XElementSourceFieldBinder.New);
        }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            return;
        }
    }
}