using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Model;
using Umbraco.Framework;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.Dynamics.Attributes;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;

namespace Umbraco.Tests.Cms
{
    [TestFixture]
    public class BendyObjectExtensionsHelperTests
    {
        private dynamic _bendy;
            
        [SetUp]
        public void SetUp()
        {
            var assemblies = new[]
            {
                typeof(Umbraco.Tests.Extensions.TestDynamicExtensions).Assembly
            };
            var supportedDynamicTypes = new[]
            {
                typeof(Content),
                typeof(BendyObject)
            };
            var supportedDynamicFieldTypes = new[]
            {
                typeof(Content),
                typeof(TypedAttribute),
                typeof(BendyObject)
            };
            var bendyMethods = DynamicExtensionsHelper.GetExtensions(assemblies, supportedDynamicTypes);
            var subBendyMethods = DynamicExtensionsHelper.GetExtensions<DynamicFieldExtensionAttribute>(assemblies, supportedDynamicFieldTypes);

            var bendy = new BendyObject();

            BendyObjectExtensionsHelper.ApplyDynamicExtensions<Content>(bendy, bendyMethods);

            var content = new Content();

            _bendy = bendy;
            _bendy["__OriginalItem"] = content;

            var attrType1 = new AttributeType {RenderTypeProvider = CorePluginConstants.FileUploadPropertyEditorId};
            var attrDef1 = new AttributeDefinition {AttributeType = attrType1};
            var attr1 = new TypedAttribute(attrDef1);

            var subBendy1 = new BendyObject();
            subBendy1["__OriginalItem"] = attr1;
            subBendy1.AddLazy("__Parent", () => bendy);
            subBendy1["__ParentKey"] = "Test";

            BendyObjectExtensionsHelper.ApplyDynamicFieldExtensions(content, CorePluginConstants.FileUploadPropertyEditorId, subBendy1, subBendyMethods);

            _bendy["Test"] = subBendy1;

            var attrType2 = new AttributeType { RenderTypeProvider = CorePluginConstants.TreeNodePickerPropertyEditorId };
            var attrDef2 = new AttributeDefinition { AttributeType = attrType2 };
            var attr2 = new TypedAttribute(attrDef2);

            var subBendy2 = new BendyObject();
            subBendy2["__OriginalItem"] = attr2;
            subBendy2.AddLazy("__Parent", () => bendy);
            subBendy2["__ParentKey"] = "Test2";

            BendyObjectExtensionsHelper.ApplyDynamicFieldExtensions(content, CorePluginConstants.TreeNodePickerPropertyEditorId, subBendy2, subBendyMethods);

            _bendy["Test2"] = subBendy2;
        }

        [Test]
        public void BendyObjectExtensionsHelperTests_ApplyDynamicExtensions_CanRunBendyObjectExtensionMethods()
        {
            // Assert
            Assert.AreEqual("Hello World", _bendy.TestBendyStringMethodNoArgs());
            Assert.AreEqual("Hello World something", _bendy.TestBendyStringMethodOneStringArg("something"));
            Assert.AreEqual("Hello World something 12 True", _bendy.TestBendyStringMethodManyArgs("something", 12, true));
        }

        [Test]
        public void BendyObjectExtensionsHelperTests_ApplyDynamicExtensions_CanRunContentExtensionMethods()
        {
            // Assert
            Assert.AreEqual("Hello World", _bendy.TestContentStringMethodNoArgs());
        }

        [Test]
        public void BendyObjectExtensionsHelperTests_ApplyDynamicFieldExtensions_CanRunContentExtensionMethods()
        {
            // Assert
            Assert.AreEqual("Hello Field World", _bendy.Test.TestName());
            Assert.AreEqual("Hello Field World", _bendy.Test.TestName2());
            Assert.AreEqual("Hello Attribute Field World", _bendy.Test.TestTypedAttributeFieldStringMethodNoArgs());
            Assert.Throws(typeof(InvalidOperationException), () => _bendy.Test2.TestTypedAttributeFieldStringMethodNoArgs()); // Test2 does not use the right prop editor, so method call should fail
        }
    }
}
