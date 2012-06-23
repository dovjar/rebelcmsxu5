namespace RebelCms.Cms.Web.Context
{
    /// <summary>
    /// Interface requiring a RoutableRequestContext
    /// </summary>
    public interface IRequiresRoutableRequestContext
    {
        IRoutableRequestContext RoutableRequestContext { get; set; }
    }
}