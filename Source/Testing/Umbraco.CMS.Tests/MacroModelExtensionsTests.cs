using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Web;

namespace Umbraco.Tests.Cms
{
    [TestFixture]
    public class MacroNameParserTests
    {

        [TestCase("", "MyController", "MyAction")]
        [TestCase("MyPackageName", "MyController", "MyAction")]
        [TestCase("My.Package.Name", "MyController", "MyAction")]
        [TestCase("My.Package-Name", "MyController", "MyAction")]
        [Test]
        public void Parse_Child_Action_Macro_Name(string area, string controller, string action)
        {
            var name = string.Concat(area, "-", controller, ".", action);
            var output = MacroNameParser.ParseChildActionMacroName(name);

            Assert.AreEqual(area, output.AreaName);
            Assert.AreEqual(controller, output.ControllerName);
            Assert.AreEqual(action, output.ActionName);
        }

    }
}
