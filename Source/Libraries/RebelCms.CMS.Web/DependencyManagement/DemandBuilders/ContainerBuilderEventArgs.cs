using System;
using RebelCms.Framework.DependencyManagement;

namespace RebelCms.Cms.Web.DependencyManagement.DemandBuilders
{
    public class ContainerBuilderEventArgs : EventArgs
    {
        public ContainerBuilderEventArgs(IContainerBuilder builder)
        {
            Builder = builder;
        }

        public IContainerBuilder Builder { get; private set; }
    }
}