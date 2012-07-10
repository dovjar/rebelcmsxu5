namespace Rebel.Cms.Web.Context
{
    /// <summary>
    /// Interface requiring an RebelHelper
    /// </summary>
    public interface IRequiresRebelHelper
    {
        RebelHelper Rebel { get; set; }
    }
}
