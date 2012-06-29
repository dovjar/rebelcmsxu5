using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Model;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.Dynamics.Attributes;
using Umbraco.Framework.Persistence.Model.Attribution;

namespace Umbraco.Tests.Extensions
{
    [DynamicExtensions]
    public static class TestDynamicExtensions
    {
        [DynamicExtension]
        public static string TestBendyStringMethodNoArgs(this BendyObject bendy)
        {
            return "Hello World";
        }

        [DynamicExtension]
        public static string TestBendyStringMethodOneStringArg(this BendyObject bendy, string arg1)
        {
            return "Hello World "+ arg1;
        }

        [DynamicExtension]
        public static string TestBendyStringMethodManyArgs(this BendyObject bendy, string arg1, int arg2, bool arg3)
        {
            return "Hello World " + arg1 + " " + arg2 + " " + arg3;
        }

        [DynamicExtension]
        public static string TestContentStringMethodNoArgs(this Content content)
        {
            return "Hello World";
        }

        [DynamicExtension]
        [DynamicFieldExtension(CorePluginConstants.FileUploadPropertyEditorId, "TestName")]
        [DynamicFieldExtension(CorePluginConstants.FileUploadPropertyEditorId, "TestName2")]
        public static string TestContentFieldStringMethodNoArgs(this Content content, string properAlias)
        {
            return "Hello Field World";
        }

        [DynamicFieldExtension(CorePluginConstants.FileUploadPropertyEditorId)]
        public static string TestTypedAttributeFieldStringMethodNoArgs(this TypedAttribute attr)
        {
            return "Hello Attribute Field World";
        }

        public static string Unreachable(this BendyObject bendy)
        {
            return "This method shouldn't be reachable by the dynamic extension helper";
        }
    }
}
