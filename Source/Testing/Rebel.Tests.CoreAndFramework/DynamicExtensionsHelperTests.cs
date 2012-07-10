using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Rebel.Cms.Web.Model;
using Rebel.Framework;
using Rebel.Framework.Dynamics;
using Rebel.Framework.Dynamics.Attributes;

namespace Rebel.Tests.CoreAndFramework
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
                typeof(Rebel.Tests.Extensions.TestDynamicExtensions).Assembly
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
