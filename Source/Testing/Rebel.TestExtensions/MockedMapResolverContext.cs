using Rebel.Cms;
using Rebel.Cms.Web;
using Rebel.Cms.Web.Context;
using Rebel.Framework.Context;
using Rebel.Hive;
using Rebel.Tests.Extensions.Stubs;

namespace Rebel.Tests.Extensions
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