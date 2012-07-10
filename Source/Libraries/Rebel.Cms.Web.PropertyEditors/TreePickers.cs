using System;
using System.Collections.Concurrent;

namespace Rebel.Cms.Web.PropertyEditors
{
    /// <summary>
    /// A static collection of trees that can be used in Tree Pickers.
    /// This collection can be modified at runtime in order to be able to add/remove trees that will be available in 
    /// any tree picker.
    /// </summary>
    public static class TreePickers
    {
        /// <summary>
        /// Constructor sets the Media and Content trees to be available by default.
        /// </summary>
        static TreePickers()
        {
            Trees = new ConcurrentBag<Guid>()
                {
                    new Guid(CorePluginConstants.ContentTreeControllerId),
                    new Guid(CorePluginConstants.MediaTreeControllerId),
                    new Guid(CorePluginConstants.MemberTreeControllerId)
                };
        }

        /// <summary>
        /// Returns the Tree Ids available to be rendered in any tree picker.
        /// </summary>
        public static ConcurrentBag<Guid> Trees { get; private set; }

    }
}