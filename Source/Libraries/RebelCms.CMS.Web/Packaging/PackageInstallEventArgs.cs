using System;
using RebelCms.Cms.Web.Model.BackOffice;

namespace RebelCms.Cms.Web.Packaging
{
    public class PackageInstallEventArgs : EventArgs
    {
        public string NugetPackageId { get; private set; }
        public PackageFolder PackageFolder { get; private set; }
        public PackageInstallationState State { get; private set; }

        public PackageInstallEventArgs(string nugetPackageId, PackageFolder packageFolder, PackageInstallationState state)
        {
            NugetPackageId = nugetPackageId;
            PackageFolder = packageFolder;
            State = state;
        }
    }
}