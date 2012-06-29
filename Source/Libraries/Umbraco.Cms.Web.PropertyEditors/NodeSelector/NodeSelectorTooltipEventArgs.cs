using System;
using Umbraco.Framework.Persistence.Model;

namespace Umbraco.Cms.Web.PropertyEditors.NodeSelector
{
    /// <summary>
    /// Used as Task argument for the GetTooltipContents task trigger
    /// </summary>
    public class NodeSelectorTooltipEventArgs : EventArgs
    {
        public NodeSelectorTooltipEventArgs(TypedEntity entity, string htmlContents)
        {
            Entity = entity;
            HtmlContents = htmlContents;
        }

        public TypedEntity Entity { get; set; }
        public string HtmlContents { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }


    }
}