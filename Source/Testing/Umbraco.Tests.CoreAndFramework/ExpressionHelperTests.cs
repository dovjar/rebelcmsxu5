using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Umbraco.Tests.CoreAndFramework
{

    [TestFixture]
    public class ExpressionHelperTests
    {

        public string MethodForTesting(string s, int i, double d, bool b)
        {
            return string.Empty;
        }


        [Test]
        public void ExpressionHelper_Get_Method_Paramaters()
        {
            var methodParams = Umbraco.Framework.ExpressionHelper.GetMethodParams<int, string>(x => MethodForTesting("h", 1, 2.0, true));
            Assert.AreEqual(4, methodParams.Count);
            Assert.AreEqual("s", methodParams.ElementAt(0).Key);
            Assert.AreEqual("i", methodParams.ElementAt(1).Key);
            Assert.AreEqual("d", methodParams.ElementAt(2).Key);
            Assert.AreEqual("b", methodParams.ElementAt(3).Key);

            Assert.AreEqual("h", methodParams.ElementAt(0).Value);
            Assert.AreEqual(1, methodParams.ElementAt(1).Value);
            Assert.AreEqual(2.0, methodParams.ElementAt(2).Value);
            Assert.AreEqual(true, methodParams.ElementAt(3).Value);
        }

    }
}
