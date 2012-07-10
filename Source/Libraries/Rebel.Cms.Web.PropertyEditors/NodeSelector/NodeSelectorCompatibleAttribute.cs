using System;
using Rebel.Framework;

namespace Rebel.Cms.Web.PropertyEditors.NodeSelector
{
    /// <summary>
    /// Any tree that is attributed with this attribute will show up in the NodeSelector available trees
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NodeSelectorCompatibleAttribute : Attribute
    {
        public Type NodeSelectorDataSourceType { get; private set; }

        public NodeSelectorCompatibleAttribute(Type nodeSelectorDataSourceType)
        {
            //validate
            if (!TypeFinder.IsTypeAssignableFrom<INodeSelectorDataSource>(nodeSelectorDataSourceType))
                throw new InvalidOperationException("The Type specified for nodeSelectorDataSourceType must be of type " + typeof(INodeSelectorDataSource).FullName);

            NodeSelectorDataSourceType = nodeSelectorDataSourceType;
        }
    }
}