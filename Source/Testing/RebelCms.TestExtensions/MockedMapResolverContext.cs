using RebelCms.Cms;
using RebelCms.Cms.Web;
using RebelCms.Cms.Web.Context;
using RebelCms.Framework.Context;
using RebelCms.Hive;
using RebelCms.Tests.Extensions.Stubs;

namespace RebelCms.Tests.Extensions
{
    public class MockedMapResolverContext : MapResolverContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public MockedMapResolverContext(IFrameworkContext frameworkContext, IHiveManager hive, IPropertyEditorFactory propertyEditorFactory, IParameterEditorFactory parameterEditorFactory)
            : base(frameworkContext, hive, propertyEditorFactory, parameterEditorFactory)
        {
        }
    }
}