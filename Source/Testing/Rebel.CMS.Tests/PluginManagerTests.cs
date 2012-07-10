using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Routing;
using NUnit.Framework;
using Rebel.Cms.Web.System;

namespace Rebel.Tests.Cms
{
    [TestFixture]
    public class PluginManagerTests
    {
        private DirectoryInfo PrepareFolder()
        {
            var assDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            var dir = Directory.CreateDirectory(Path.Combine(assDir.FullName, "PluginManager", Guid.NewGuid().ToString("N")));
            foreach(var f in dir.GetFiles())
            {
                f.Delete();
            }
            return dir;
        }

        [Test]
        public void PluginHash_From_String()
        {
            var s = "hello my name is someone".GetHashCode().ToString("x", CultureInfo.InvariantCulture);
            var output = PluginManager.ConvertPluginsHash(s);
            Assert.AreNotEqual(0, output);
        }

        [Test]
        public void Cleanup_DotDelete_Plugin_Files()
        {
            //Arrange
            var dir = PrepareFolder();
            (new StreamWriter(Path.Combine(dir.FullName, "test.dll"))).Close();
            (new StreamWriter(Path.Combine(dir.FullName, "test.dll.delete"))).Close();
            (new StreamWriter(Path.Combine(dir.FullName, "test.dll" + Guid.NewGuid().ToString("N") + ".delete"))).Close();
            (new StreamWriter(Path.Combine(dir.FullName, "willnotdelete.dll"))).Close();

            //Act
            PluginManager.CleanupDotDeletePluginFiles(new FileInfo(Path.Combine(dir.FullName, "test.dll.delete")));

            //Assert
            dir.Refresh();
            Assert.AreEqual(1, dir.GetFiles().Count());
        }

        [Test]
        public void Get_Plugins_Hash()
        {
            //Arrange
            var dir = PrepareFolder();
            var d1 = dir.CreateSubdirectory("1");
            var d2 = dir.CreateSubdirectory("2");
            var d3 = dir.CreateSubdirectory("3");
            var d4 = dir.CreateSubdirectory("4");
            var f1 = new FileInfo(Path.Combine(d1.FullName, "test1.dll"));
            var f2 = new FileInfo(Path.Combine(d1.FullName, "test2.dll"));
            var f3 = new FileInfo(Path.Combine(d2.FullName, "test1.dll"));
            var f4 = new FileInfo(Path.Combine(d2.FullName, "test2.dll"));
            var f5 = new FileInfo(Path.Combine(d3.FullName, "test1.dll"));
            var f6 = new FileInfo(Path.Combine(d3.FullName, "test2.dll"));
            var f7 = new FileInfo(Path.Combine(d4.FullName, "test1.dll"));
            f1.CreateText().Close();
            f2.CreateText().Close();
            f3.CreateText().Close();
            f4.CreateText().Close();
            f5.CreateText().Close();
            f6.CreateText().Close();
            f7.CreateText().Close();
            var list1 = new[] {f1, f2, f3, f4, f5, f6};
            var list2 = new[] { f1, f3, f5 };
            var list3 = new[] { f1, f3, f5, f7 };

            //Act
            var hash1 = PluginManager.GetPluginsHash(list1, true);
            var hash2 = PluginManager.GetPluginsHash(list2, true);
            var hash3 = PluginManager.GetPluginsHash(list3, true);

            //Assert

            //both should be the same since we only create the hash based on the unique folder of the list passed in, yet
            //all files will exist in those folders still
            Assert.AreEqual(hash1, hash2);
            Assert.AreNotEqual(hash1, hash3);
        }

    }
}