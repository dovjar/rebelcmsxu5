using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rebel.Cms.Web;
using Rebel.Framework;

namespace Rebel.Tests.Cms
{
    [TestFixture]
    public class LinkSyntaxParserTests
    {
        [Test]
        public void LinkSyntaxParser_Parses_Links()
        {
            var parser = new LinkSyntaxParser();
            var output = parser.Parse(@"<p>Lorem <a href=""" + HiveId.Empty.ToString() + @""" title=""ipsum"" data-rebel-link=""internal"">ipsum</a> dolar <a href=""" + HiveId.Empty.ToString() + @""" title=""sit"" data-rebel-link=""internal"">sit</a> amet.</p>",
                          (hiveId) => "replacement_link");

            Assert.IsTrue(output == @"<p>Lorem <a href=""replacement_link"" title=""ipsum"" >ipsum</a> dolar <a href=""replacement_link"" title=""sit"" >sit</a> amet.</p>");
        }


        [Test]
        public void LinkSyntaxParser_Does_Not_Parses_Links()
        {
            var parser = new LinkSyntaxParser();
            var output = parser.Parse("<p><a href=\"http://www.linkedsite.com\"><img src=\"../../../../Content/Media/e74f82351b0b4a35819e6373e3fbfddc/testimage.png\" alt=\"linked site\" width=\"170\" height=\"64\" data-rebel-link=\"internal\" data-rebel-src=\"media$empty_root$$_p__nhibernate$_v__guid$_c2f2015e7e724d2a822a9ff000f20224\" /></a></p>",
                          (hiveId) => "replacement_link");

            Assert.IsTrue(output == "<p><a href=\"http://www.linkedsite.com\"><img src=\"replacement_link\" alt=\"linked site\" width=\"170\" height=\"64\"   /></a></p>");
        }
    }
}
