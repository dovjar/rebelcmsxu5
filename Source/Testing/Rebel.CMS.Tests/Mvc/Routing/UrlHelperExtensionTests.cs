using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rebel.Cms.Web;
using Rebel.Cms.Web.Dashboards.Filters;
using Rebel.Cms.Web.Dashboards.Rules;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.Macros;
using Rebel.Cms.Web.Model.BackOffice.ParameterEditors;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.Areas;
using Rebel.Cms.Web.Routing;
using Rebel.Cms.Web.System;
using Rebel.Cms.Web.System.Boot;
using Rebel.Cms.Web.Trees;
using Rebel.Framework.Persistence;
using Rebel.Framework.Security;
using Rebel.Framework.Tasks;
using Rebel.Framework.Testing;
using Rebel.Tests.Cms.Stubs;
using Rebel.Tests.Extensions;

namespace Rebel.Tests.Cms.Mvc.Routing
{
    [TestFixture]
    public class UrlHelperExtensionTests : AbstractRoutingTest
    {
      
        private UrlHelper CreateHelper()
        {
            var http = new FakeHttpContextFactory("~/test");
            var url = new UrlHelper(http.RequestContext);
            return url;
        }

        [Test]
        public void CreateTreeParams()
        {
            var urlHelper = CreateHelper();
            var output = urlHelper.CreateTreeParams(
                true, 
                "test.this", 
                new Dictionary<string, object> {{TreeQueryStringParameters.RenderParent, true}});

            Assert.AreEqual(3, output.Count);
            Assert.IsTrue(output.ContainsKey(TreeQueryStringParameters.DialogMode));
            Assert.IsTrue(output.ContainsKey(TreeQueryStringParameters.RenderParent));
            Assert.IsTrue(output.ContainsKey(TreeQueryStringParameters.OnNodeClick));
        }

    }
}