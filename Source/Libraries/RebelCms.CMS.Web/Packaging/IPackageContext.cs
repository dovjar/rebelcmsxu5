using NuGet;

namespace RebelCms.Cms.Web.Packaging
{
    public interface IPackageContext
    {
        IPackageManager LocalPackageManager { get; }
        IPackageManager PublicPackageManager { get; }
        IPackagePathResolver LocalPathResolver { get; }
    }
}