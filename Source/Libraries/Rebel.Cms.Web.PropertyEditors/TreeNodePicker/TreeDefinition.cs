using System;
using Rebel.Framework;

namespace Rebel.Cms.Web.PropertyEditors.TreeNodePicker
{

    /// <summary>
    /// Defines a tree, both the name and the Id
    /// </summary>
    public class TreeDefinition
    {
        public Guid TreeControllerId { get; set; }
        public string TreeControllerName { get; set; }

    }
}