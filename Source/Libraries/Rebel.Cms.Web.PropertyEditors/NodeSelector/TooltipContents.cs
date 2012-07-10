namespace Rebel.Cms.Web.PropertyEditors.NodeSelector
{

    /// <summary>
    /// The response object for setting the tooltip content
    /// </summary>
    public class TooltipContents
    {
        
        public TooltipContents(string htmlContents)
        {
            Width = -1;
            Height = -1;
            HtmlContent = htmlContents;
        }

        /// <summary>
        /// The content to display in the tooltip
        /// </summary>
        public string HtmlContent { get; set; }

        /// <summary>
        /// The width of the tooltip to render, if not set, will remain the default
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of the tooltip to render, if not set, will remain the default
        /// </summary>
        public int Height { get; set; }
    }
}