using System;
using System.Collections.Generic;
using System.Web;
using Rebel.Cms;
using Rebel.Cms.Web;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Dashboards.Filters;
using Rebel.Cms.Web.Dashboards.Rules;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.Editors;
using Rebel.Cms.Web.Macros;
using Rebel.Cms.Web.Model;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Model.BackOffice.ParameterEditors;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Routing;
using Rebel.Cms.Web.Surface;
using Rebel.Cms.Web.Trees;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Framework.Security;
using Rebel.Framework.Tasks;
using Rebel.Tests.Extensions.Stubs;

namespace Rebel.Tests.Extensions
{
    public class FakeRoutableRequestContext : IRoutableRequestContext
    {
        private IRoutingEngine _engine;

        public FakeRoutableRequestContext(IRebelApplicationContext application)
        {
            Application = application;
        }

        public FakeRoutableRequestContext(IRebelApplicationContext application, IRoutingEngine engine)
        {
            _engine = engine;
            Application = application;
        }

        public FakeRoutableRequestContext()
            : this(new FakeRebelApplicationContext())
        { }

        private Guid? _requestId = null;

        /// <summary>
        /// Gets the request id, useful for debugging or tracing.
        /// </summary>
        /// <value>The request id.</value>
        public Guid RequestId
        {
            get
            {
                if (_requestId == null)
                    _requestId = Guid.NewGuid();
                return _requestId.Value;
            }
        }

        /// <summary>
        /// Gets the Rebel application context which contains services which last for the lifetime of the application.
        /// </summary>
        /// <value>The application.</value>
        public IRebelApplicationContext Application { get; protected set; }


        public ComponentRegistrations RegisteredComponents
        {
            get
            {
                var propEditors = new FixedPropertyEditors(Application);

                return new ComponentRegistrations(new List<Lazy<MenuItem, MenuItemMetadata>>(),
                                                  new List<Lazy<AbstractEditorController, EditorMetadata>>(),
                                                  new List<Lazy<TreeController, TreeMetadata>>(),
                                                  new List<Lazy<SurfaceController, SurfaceMetadata>>(),
                                                  new List<Lazy<AbstractTask, TaskMetadata>>(),
                                                  propEditors.GetPropertyEditorDefinitions(),
                                                  new List<Lazy<AbstractParameterEditor, ParameterEditorMetadata>>(),
                                                  new List<Lazy<DashboardMatchRule, DashboardRuleMetadata>>(),
                                                  new List<Lazy<DashboardFilter, DashboardRuleMetadata>>(),
                                                  new List<Lazy<Permission, PermissionMetadata>>(),
                                                  new List<Lazy<AbstractMacroEngine, MacroEngineMetadata>>());
            }
        }

        public IRoutingEngine RoutingEngine
        {
            get
            {
                if (_engine == null)
                {
                    _engine = new FakeRoutingEngine();
                }
                return _engine;
            }
        }

    }
}
