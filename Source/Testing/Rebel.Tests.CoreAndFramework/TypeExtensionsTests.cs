using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Framework.Security.Model.Entities;

namespace Rebel.Tests.CoreAndFramework
{
    [TestFixture]
    public class TypeExtensionsTests
    {
        private class MyCustomCollection : List<string>
        {
            
        }

        private class MyUserRevision : Revision<User>
        {
            
        }

        [Test]
        public void TypeExtensions_Can_Get_IList_Underlying_Type()
        {
            Type[] genericArgTypes;
            if (typeof(MyCustomCollection).TryGetGenericArguments(typeof(IList<>), out genericArgTypes))
            {
                Assert.AreEqual(typeof(string), genericArgTypes[0]);
            }
            else
            {
                Assert.Fail();    
            }
        }

        [Test]
        public void TypeExtensions_Can_Get_Revision_Underlying_Type()
        {
            Type[] genericArgTypes;
            if (typeof(Revision<User>).TryGetGenericArguments(typeof(Revision<>), out genericArgTypes))
            {
                Assert.AreEqual(typeof(User), genericArgTypes[0]);
            }
            else
            {
                Assert.Fail();
            }

            if (typeof(MyUserRevision).TryGetGenericArguments(typeof(Revision<>), out genericArgTypes))
            {
                Assert.AreEqual(typeof(User), genericArgTypes[0]);
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}
