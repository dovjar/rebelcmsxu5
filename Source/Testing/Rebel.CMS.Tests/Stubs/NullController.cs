using NUnit.Framework;
using System.Reflection;
using Rebel.Cms.Web.Dashboards;
using Rebel.Cms.Web.Dashboards.Filters;
using Rebel.Cms.Web.Dashboards.Rules;
using Rebel.Cms.Web.Macros;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Model.BackOffice.ParameterEditors;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;
using Rebel.Cms.Web.Routing;
using Rebel.Cms.Web.System;
using Rebel.Cms.Web.System.Boot;
using Rebel.Cms.Web.UI;
using Rebel.Framework.Persistence;
using Rebel.Framework.Security;
using Rebel.Framework.Testing;
using Rebel.Tests.Cms.Stubs;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.Mvc.Areas;
using System.Web.Routing;
using System.Web.Mvc;
using Rebel.Framework.Tasks;
using Rebel.Tests.Extensions;
using Rebel.Tests.Extensions.Stubs;

namespace Rebel.Tests.Cms.Stubs
{
    /// <summary>
    /// Represents an empty controller that can be used in tests
    /// </summary>
    public class NullController : Controller
    {
        
    }
}