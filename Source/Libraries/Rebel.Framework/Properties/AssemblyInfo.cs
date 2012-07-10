using System.Reflection;
using System.Runtime.CompilerServices;
using Rebel.Framework.Localization.Configuration;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("Rebel.Framework")]
[assembly: AssemblyDescription("Core assembly for the Rebel framework")]
[assembly: AssemblyConfiguration("")]


[assembly: LocalizationXmlSource("Localization.Defaults.Enumerations.xml")]
[assembly: LocalizationXmlSource("Localization.Defaults.Time.xml")]
[assembly: LocalizationXmlSource("Localization.Defaults.Exceptions.xml")]
//TODO: Plural forms. http://translate.sourceforge.net/wiki/l10n/pluralforms
[assembly: LocalizationXmlSource("Localization.Defaults.PluralForms.xml")]

[assembly: InternalsVisibleTo("Rebel.Tests.Framework")]
[assembly: InternalsVisibleTo("Rebel.Tests.CoreAndFramework")]
[assembly: InternalsVisibleTo("Rebel.Cms.Web")]
