using System;
using System.Web;
using Rebel.Cms;
using Rebel.Cms.Web;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.IO;
using Rebel.Cms.Web.Model;
using Rebel.Cms.Web.Packaging;
using Rebel.Cms.Web.Routing;

namespace Rebel.Tests.Extensions
{
    public class FakeBackOfficeRequestContext : FakeRoutableRequestContext, IBackOfficeRequestContext
    {
        public FakeBackOfficeRequestContext(IRebelApplicationContext application, IRoutingEngine engine)
            : base(application, engine)
        {
            
        }

        public FakeBackOfficeRequestContext(IRebelApplicationContext application)
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