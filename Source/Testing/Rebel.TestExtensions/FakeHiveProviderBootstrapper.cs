using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Rebel.Framework.Dynamics;
using Rebel.Framework.ProviderSupport;
using AbstractProviderBootstrapper = Rebel.Hive.AbstractProviderBootstrapper;

namespace Rebel.Tests.Extensions
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
