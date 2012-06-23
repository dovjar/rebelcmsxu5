
namespace RebelCms.Framework
{
    /// <summary>
    /// Defines that an object be suitable for dependency injection.
    /// </summary>
    public interface ISupportsProviderInjection
    {
        IProviderManifest Provider { get; set; }
    }
}
