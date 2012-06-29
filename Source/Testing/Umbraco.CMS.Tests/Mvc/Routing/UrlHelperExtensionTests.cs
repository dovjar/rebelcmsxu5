using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Dashboards.Filters;
using Umbraco.Cms.Web.Dashboards.Rules;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Macros;
using Umbraco.Cms.Web.Model.BackOffice.ParameterEditors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.Areas;
using Umbraco.Cms.Web.Routing;
using Umbraco.Cms.Web.System;
using Umbraco.Cms.Web.System.Boot;
using Umbraco.Cms.Web.Trees;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Security;
using Umbraco.Framework.Tasks;
using Umbraco.Framework.Testing;
using Umbraco.Tests.Cms.Stubs;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.Cms.Mvc.Routing
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