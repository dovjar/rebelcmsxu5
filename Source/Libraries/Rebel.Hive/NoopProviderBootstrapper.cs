using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Rebel.Framework.Dynamics;
using Rebel.Framework.ProviderSupport;

namespace Rebel.Hive
{
    public class NoopProviderBootstrapper : AbstractProviderBootstrapper
    {
        public override void ConfigureApplication(string providerKey, XDocument configXml, BendyObject installParams)
        {
            return;
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
