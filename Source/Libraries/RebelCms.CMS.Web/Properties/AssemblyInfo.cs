using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web.UI;
using RebelCms.Cms.Web;
using RebelCms.Framework.Localization.Configuration;

[assembly: AssemblyTitle("RebelCms.Cms.Web")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyContainsPlugins]

[assembly: InternalsVisibleTo("RebelCms.Tests.DomainDesign")]
[assembly: InternalsVisibleTo("RebelCms.Cms.Web")]
[assembly: InternalsVisibleTo("RebelCms.Cms.Web.UI")]
[assembly: InternalsVisibleTo("RebelCms.Tests.Cms")]
[assembly: InternalsVisibleTo("RebelCms.Cms.Web.Editors")]
[assembly: InternalsVisibleTo("RebelCms.Cms.Web.Trees")]
[assembly: InternalsVisibleTo("RebelCms.Tests.Extensions")]
[assembly: InternalsVisibleTo("RebelCms.Cms.Web.PropertyEditors")]
[assembly: InternalsVisibleTo("RebelCms.Cms.Packages.DevDataset")]
[assembly: InternalsVisibleTo("RebelCms.Cms.Web.Tasks")]
[assembly: InternalsVisibleTo("RebelCms.Tests.Cms.DomainIntegration")]

[assembly: LocalizationXmlSource("Localization.Default.xml")]

[assembly: WebResource("RebelCms.Cms.Web.EmbeddedViews.Views.Resources.Site.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("RebelCms.Cms.Web.EmbeddedViews.Views.Resources.RebelCms-logo.png", "image/png")]

[assembly: InternalsVisibleTo("RebelCms.Tests.CoreAndFramework")]