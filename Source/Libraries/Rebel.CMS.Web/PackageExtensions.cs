using NuGet;

namespace Rebel.Cms.Web
{
    public static class PackageExtensions
    {
        public static bool IsInstalled(this IPackageMetadata package, IPackageRepository repo)
        {
            if (package != null && package.Id != null)
                return repo.FindPackage(package.Id) != null;
            return false;
        }

    }
}
