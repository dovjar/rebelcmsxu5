using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Compilation;
using System.Web.Mvc;
using NUnit.Framework;
using Rebel.Cms.Web.BuildManagerCodeDelegates;
using Rebel.Cms.Web.PropertyEditors.NodeSelector;
using Rebel.Framework;

namespace Rebel.Tests.Cms.PropertyEditors.NodeSelector
{
    [TestFixture]
    public class DelegateVirtualPathProviderTests
    {
        [SetUp]
        public void SetupTest()
        {
            CodeDelegatesCollection.Clear();
            CodeDelegateVirtualPath.ClearPathIds();
        }

        [Test]
        public void Get_File()
        {
            const string testSnippet = @"this is just a test {0}";
            var virtualPathId = CodeDelegateVirtualPath.GetOrCreateVirtualPathId("MyKey");
            CodeDelegatesCollection.TryAdd(virtualPathId, (dp, vpi) => string.Format(testSnippet, dp));
            var path = CodeDelegateVirtualPath.CreateFullPath(virtualPathId, "cs").TrimStart("~");
            var provider = new CodeDelegateVirtualPathProvider();
            var file = provider.GetFile(path);
            Assert.AreEqual(typeof(CodeDelegateVirtualFile), file.GetType());
        }

        [Test]
        public void Open_File()
        {
            const string testSnippet = @"this is just a test {0}";
            var virtualPathId = CodeDelegateVirtualPath.GetOrCreateVirtualPathId("MyKey");
            CodeDelegatesCollection.TryAdd(virtualPathId, (dp, vpi) => string.Format(testSnippet, dp));

            var def = CodeDelegateVirtualPath.TryGetVirtualPathDefinition(virtualPathId);
            
            var path = CodeDelegateVirtualPath.CreateFullPath(virtualPathId, "cs");
            var file = new CodeDelegateVirtualFile(virtualPathId, def.Result.Value.Parameter, path);
            using(var sr = new StreamReader(file.Open()))
            {
                Assert.AreEqual(string.Format(testSnippet, "MyKey"), sr.ReadToEnd());
            }
        }

        [Test]
        public void Is_Delegate_Id()
        {
            //first, don't add, but it is valid
            var id = "_DVP_u_guid____00000000000000000000000000000010_NodeFilter";
            Assert.IsFalse(CodeDelegateVirtualPath.IsVirtualPathId(id));

            //now, add it to the collection
            var delegateParameter = new Tuple<HiveId, CSharpCodeBlockType>(HiveId.ConvertIntToGuid(10), CSharpCodeBlockType.NodeFilter);
            var virtualPathId = delegateParameter.Item1.GetHtmlId() + "_" + delegateParameter.Item2;
            CodeDelegateVirtualPath.GetOrCreateVirtualPathId(delegateParameter, virtualPathId);

            //this should pass now that it is added
            Assert.IsTrue(CodeDelegateVirtualPath.IsVirtualPathId(id));   
        }

        [Test]
        public void Is_Delegate_Path()
        {
            //first, don't add, but it is valid
            var path = "/DVP.axd/_DVP_u_guid____00000000000000000000000000000010_NodeFilter.cs";
            Assert.IsFalse(CodeDelegateVirtualPath.IsCodeDelegateVirtualPath(path));
            
            //now, add it to the collection
            var delegateParameter = new Tuple<HiveId, CSharpCodeBlockType>(HiveId.ConvertIntToGuid(10), CSharpCodeBlockType.NodeFilter);
            var virtualPathId = delegateParameter.Item1.GetHtmlId() + "_" + delegateParameter.Item2;
            CodeDelegateVirtualPath.GetOrCreateVirtualPathId(delegateParameter, virtualPathId);

            //this should pass now that it is added
            Assert.IsTrue(CodeDelegateVirtualPath.IsCodeDelegateVirtualPath(path));            
        }

        [Test]
        public void Create_Path_Id()
        {
            var delegateParameter = new Tuple<HiveId, CSharpCodeBlockType>(HiveId.ConvertIntToGuid(10), CSharpCodeBlockType.NodeFilter);
            var virtualPathId = delegateParameter.Item1.GetHtmlId() + "_" + delegateParameter.Item2;
            var pathId = CodeDelegateVirtualPath.GetOrCreateVirtualPathId(delegateParameter, virtualPathId);
            Assert.AreEqual("_DVP_u_guid____00000000000000000000000000000010_NodeFilter", pathId);
        }

        [Test]
        public void Get_Full_Path()
        {
            var delegateParameter = new Tuple<HiveId, CSharpCodeBlockType>(HiveId.ConvertIntToGuid(10), CSharpCodeBlockType.NodeFilter);
            var virtualPathId = delegateParameter.Item1.GetHtmlId() + "_" + delegateParameter.Item2;
            var path = CodeDelegateVirtualPath.CreateFullPath(
                CodeDelegateVirtualPath.GetOrCreateVirtualPathId(delegateParameter, virtualPathId), "cs");
            Assert.AreEqual("~/DVP.axd/_DVP_u_guid____00000000000000000000000000000010_NodeFilter.cs", path);
        }

        [Test]
        public void Created_Path_Passes_Microsoft_VirtualPath_Parser()
        {
            var delegateParameter = new Tuple<HiveId, CSharpCodeBlockType>(new HiveId(Guid.NewGuid()), CSharpCodeBlockType.NodeFilter);
            var virtualPathId =  delegateParameter.Item1.GetHtmlId() + "_" + delegateParameter.Item2;
            var path = CodeDelegateVirtualPath.CreateFullPath(
                CodeDelegateVirtualPath.GetOrCreateVirtualPathId(delegateParameter, virtualPathId), "cs");

            try
            {
                var result = BuildManager.CreateInstanceFromVirtualPath(path, typeof(WebViewPage));
            }
            catch (Exception ex)
            {
                //this is the exception we are trying to avoid
                if (ex is ArgumentException && ex.Message.StartsWith("The relative virtual path") && ex.Message.EndsWith("is not allowed here."))
                {
                    throw;
                }
            }

            //no exception was thrown
            Assert.Pass();
        }

    }
}
