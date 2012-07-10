using System.IO;
using Rebel.Cms.Packages.DevDataset;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Framework.Context;
using Rebel.Framework.Persistence;

namespace Rebel.Tests.Extensions
{
    public static class DemoDataHelper
    {
        public static DevDataset GetDemoData(IRebelApplicationContext appContext, IAttributeTypeRegistry attributeTypeRegistry)
        {
            return new DevDataset(new MockedPropertyEditorFactory(appContext), appContext.FrameworkContext, attributeTypeRegistry);
        }
    }
}