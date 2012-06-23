using System.IO;
using RebelCms.Cms.Packages.DevDataset;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.DependencyManagement;
using RebelCms.Framework.Context;
using RebelCms.Framework.Persistence;

namespace RebelCms.Tests.Extensions
{
    public static class DemoDataHelper
    {
        public static DevDataset GetDemoData(IRebelCmsApplicationContext appContext, IAttributeTypeRegistry attributeTypeRegistry)
        {
            return new DevDataset(new MockedPropertyEditorFactory(appContext), appContext.FrameworkContext, attributeTypeRegistry);
        }
    }
}