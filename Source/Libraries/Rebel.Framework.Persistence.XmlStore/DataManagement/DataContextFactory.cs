using System;
using System.Xml.Linq;

using Rebel.Framework.Context;
using Rebel.Framework.DataManagement;
using Rebel.Framework.Persistence.DataManagement;
using Rebel.Framework.Persistence.ProviderSupport;
using Rebel.Framework.Persistence.XmlStore.DataManagement.Linq;

namespace Rebel.Framework.Persistence.XmlStore.DataManagement
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