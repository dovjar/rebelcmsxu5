namespace Rebel.Cms.Web.Model
{
    using Rebel.Framework;

    /// <summary>
    /// The model supplied to the controller to render the content to the front-end.
    /// This model wraps the 'Content' model which is returned by the controller action to the view
    /// and should generally return the 'ContentNode' as Lazy loaded data.
    /// </summary>
    public interface IRebelRenderModel
    {
        /// <summary>
        /// Gets the current item associated with this request.
        /// </summary>
        /// <value>The current item.</value>
        Content CurrentNode { get; }

        /// <summary>
        /// Gets a value indicating whether the current node has been loaded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is loaded; otherwise, <c>false</c>.
        /// </value>
        bool IsLoaded { get; }
    }
}