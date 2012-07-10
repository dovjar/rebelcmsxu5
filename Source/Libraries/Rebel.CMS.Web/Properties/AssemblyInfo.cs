using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web.UI;
using Rebel.Cms.Web;
using Rebel.Framework.Localization.Configuration;

[assembly: AssemblyTitle("Rebel.Cms.Web")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyContainsPlugins]

[assembly: InternalsVisibleTo("Rebel.Tests.DomainDesign")]
[assembly: InternalsVisibleTo("Rebel.Cms.Web")]
[assembly: InternalsVisibleTo("Rebel.Cms.Web.UI")]
[assembly: InternalsVisibleTo("Rebel.Tests.Cms")]
[assembly: InternalsVisibleTo("Rebel.Cms.Web.Editors")]
[assembly: InternalsVisibleTo("Rebel.Cms.Web.Trees")]
[assembly: InternalsVisibleTo("Rebel.Tests.Extensions")]
[assembly: InternalsVisibleTo("Rebel.Cms.Web.PropertyEditors")]
[assembly: InternalsVisibleTo("Rebel.Cms.Packages.DevDataset")]
[assembly: InternalsVisibleTo("Rebel.Cms.Web.Tasks")]
[assembly: InternalsVisibleTo("Rebel.Tests.Cms.DomainIntegration")]

[assembly: LocalizationXmlSource("Localization.Default.xml")]

[assembly: WebResource("Rebel.Cms.Web.EmbeddedViews.Views.Resources.Site.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("Rebel.Cms.Web.EmbeddedViews.Views.Resources.rebel-logo.png", "image/png")]

[assembly: InternalsVisibleTo("Rebel.Tests.CoreAndFramework")]