
using RebelCms.Framework.Context;
using RebelCms.Framework.DataManagement;
using RebelCms.Framework.Persistence.DataManagement;
using RebelCms.Framework.Persistence.ProviderSupport;

namespace RebelCms.Framework.IO
{
    public class DataContextFactory : AbstractDataContextFactory
    {
        private readonly string _supportedExtensions;
        private readonly string _rootPath;
        private readonly string _applicationRelativePath;
        private readonly string _excludeExtensions;

        public DataContextFactory(string supportedExtensions, string rootPath, string applicationRelativePath, string excludeExtensions)
        {
            _supportedExtensions = supportedExtensions;
            _rootPath = rootPath;
            _applicationRelativePath = applicationRelativePath;
            _excludeExtensions = excludeExtensions;
        }

        protected override void DisposeResources()
        {
        }

        protected override AbstractDataContext InstantiateDataContext(IHiveProvider hiveProvider)
        {
            return new DataContext(
                hiveProvider,
                _supportedExtensions,
                _rootPath,
                _applicationRelativePath,
                _excludeExtensions);
        }
    }
}
