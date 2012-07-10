using System;
using System.IO;
using System.Reflection;
using Rebel.Framework.Testing;
using log4net.Config;

using Rebel.Framework;

namespace Rebel.Tests.Extensions
{
    public static class TestHelper
    {
        public static void SetupLog4NetForTests()
        {
            XmlConfigurator.Configure(new FileInfo(Common.MapPathForTest("~/unit-test-log4net.config")));
        }
    }
}