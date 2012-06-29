using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Web.Model;
using Umbraco.Framework;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.Dynamics.Attributes;

namespace Umbraco.Tests.CoreAndFramework
{
    [TestFixture]
    public class DynamicExtensionsHelperTests
    {
        [Test]
        public void DynamicExtensionsHelperTests_GetExtensions_ReturnsResults()
        {
            // Arrange
            var assemblies = new []
            {
                typeof(Umbraco.Tests.Extensions.TestDynamicExtensions).Assembly
            };
            var supportedTypes = new[]
            {
                typeof(Content),
                typeof(BendyObject)
            };

            // Act
            var methods = DynamicExtensionsHelper.GetExtensions(assemblies, supportedTypes);

            // Assert
            Assert.IsTrue(methods.Any());
        }
    }
}
