using System;
using System.Web;
using RebelCms.Cms;
using RebelCms.Cms.Web;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.IO;
using RebelCms.Cms.Web.Model;
using RebelCms.Cms.Web.Packaging;
using RebelCms.Cms.Web.Routing;

namespace RebelCms.Tests.Extensions
{
    public class FakeBackOfficeRequestContext : FakeRoutableRequestContext, IBackOfficeRequestContext
    {
        public FakeBackOfficeRequestContext(IRebelCmsApplicationContext application, IRoutingEngine engine)
            : base(application, engine)
        {
            
        }

        public FakeBackOfficeRequestContext(IRebelCmsApplicationContext application)
            : base(application)
        {
        }

        public FakeBackOfficeRequestContext()
        {
        }

        public SpriteIconFileResolver DocumentTypeIconResolver
        {
            get { return new MockedIconFileResolver(); }
        }

        public SpriteIconFileResolver ApplicationIconResolver
        {
            get { return new MockedIconFileResolver(); }
        }

        public IResolver<Icon> DocumentTypeThumbnailResolver
        {
            get { return new MockedIconFileResolver(); }
        }

        public IPackageContext PackageContext
        {
            get { throw new NotImplementedException(); }
        }
    }
}