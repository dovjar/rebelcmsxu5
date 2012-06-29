using NUnit.Framework;
using System.Reflection;
using Umbraco.Cms.Web.Dashboards;
using Umbraco.Cms.Web.Dashboards.Filters;
using Umbraco.Cms.Web.Dashboards.Rules;
using Umbraco.Cms.Web.Macros;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.ParameterEditors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Routing;
using Umbraco.Cms.Web.System;
using Umbraco.Cms.Web.System.Boot;
using Umbraco.Cms.Web.UI;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Security;
using Umbraco.Framework.Testing;
using Umbraco.Tests.Cms.Stubs;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Mvc.Areas;
using System.Web.Routing;
using System.Web.Mvc;
using Umbraco.Framework.Tasks;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.Stubs;

namespace Umbraco.Tests.Cms.Stubs
{
    /// <summary>
    /// Represents an empty controller that can be used in tests
    /// </summary>
    public class NullController : Controller
    {
        
    }
}