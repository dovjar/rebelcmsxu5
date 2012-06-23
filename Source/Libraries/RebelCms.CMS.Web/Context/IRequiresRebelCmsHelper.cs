namespace RebelCms.Cms.Web.Context
{
    /// <summary>
    /// Interface requiring an RebelCmsHelper
    /// </summary>
    public interface IRequiresRebelCmsHelper
    {
        RebelCmsHelper RebelCms { get; set; }
    }
}
