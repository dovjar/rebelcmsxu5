using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Trees;
using Rebel.Framework;

namespace Rebel.Cms.Web.PropertyEditors.NodeSelector
{
    public static class TreePluginExtensions
    {
        /// <summary>
        /// Returns the INodeSelectorDataSource for the tree requested, if the tree does not support this then an exception will be thrown.
        /// </summary>
        /// <param name="registeredTrees"></param>
        /// <param name="treeId"></param>
        /// <returns></returns>
        public static INodeSelectorDataSource GetNodeSelectorDataSource(this IEnumerable<Lazy<TreeController, TreeMetadata>> registeredTrees, Guid treeId)
        {
            var tree = registeredTrees
                .Where(x => x.Metadata.Id == treeId)
                .SingleOrDefault();
            if (tree == null)
                throw new InvalidOperationException("Could not find the tree in the registered tree collections with the specified id: " + treeId);
            //now we have the tree, we need get the NodeSelectorCompatibleAttribute to obtain our INodeSelectorDataSource
            var nsCompatibleAttr = tree.Metadata.ComponentType.GetCustomAttributes<NodeSelectorCompatibleAttribute>(false).SingleOrDefault();
            if (nsCompatibleAttr == null)
                throw new InvalidOperationException("The tree type: " + tree.Metadata.ComponentType.FullName + " is not attributed with: " + typeof(NodeSelectorCompatibleAttribute).FullName + " which is a requirement of NodeSelector");
            //now we need to resolve the model resolver from IoC
            var dataSource = DependencyResolver.Current.GetService(nsCompatibleAttr.NodeSelectorDataSourceType);
            if (dataSource == null)
                throw new InvalidOperationException("The " + typeof(INodeSelectorDataSource).Name + " type: " + nsCompatibleAttr.NodeSelectorDataSourceType.FullName + " could not be resolved from the DependencyResolver, ensure it is added to the IoC container on startup.");
            return (INodeSelectorDataSource)dataSource;
        }

    }
}