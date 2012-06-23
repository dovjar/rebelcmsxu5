using System;
using System.Collections.Generic;
using System.Web;
using RebelCms.Cms;
using RebelCms.Cms.Web;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Dashboards.Filters;
using RebelCms.Cms.Web.Dashboards.Rules;
using RebelCms.Cms.Web.DependencyManagement;
using RebelCms.Cms.Web.Editors;
using RebelCms.Cms.Web.Macros;
using RebelCms.Cms.Web.Model;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Cms.Web.Model.BackOffice.ParameterEditors;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Routing;
using RebelCms.Cms.Web.Surface;
using RebelCms.Cms.Web.Trees;
using RebelCms.Framework;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Versioning;
using RebelCms.Framework.Security;
using RebelCms.Framework.Tasks;
using RebelCms.Tests.Extensions.Stubs;

namespace RebelCms.Tests.Extensions
{
    public class FakeRoutableRequestContext : IRoutableRequestContext
    {
        private IRoutingEngine _engine;

        public FakeRoutableRequestContext(IRebelCmsApplicationContext application)
        {
            Application = application;
        }

        public FakeRoutableRequestContext(IRebelCmsApplicationContext application, IRoutingEngine engine)
        {
            _engine = engine;
            Application = application;
        }

        public FakeRoutableRequestContext()
            : this(new FakeRebelCmsApplicationContext())
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
        /// Gets the RebelCms application context which contains services which last for the lifetime of the application.
        /// </summary>
        /// <value>The application.</value>
        public IRebelCmsApplicationContext Application { get; protected set; }


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
