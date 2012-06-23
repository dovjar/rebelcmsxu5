using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RebelCms.Framework.Dynamics;
using RebelCms.Framework.ProviderSupport;
using AbstractProviderBootstrapper = RebelCms.Hive.AbstractProviderBootstrapper;

namespace RebelCms.Tests.Extensions
{
    public class FakeHiveProviderBootstrapper : AbstractProviderBootstrapper
    {
        public override void ConfigureApplication(string providerKey, XDocument configXml, BendyObject installParams)
        {
            
        }

        public override InstallStatus GetInstallStatus()
        {
            return new InstallStatus(InstallStatusType.Completed);
        }

        public override InstallStatus TryInstall()
        {
            return new InstallStatus(InstallStatusType.Completed);
        }
    }
}
