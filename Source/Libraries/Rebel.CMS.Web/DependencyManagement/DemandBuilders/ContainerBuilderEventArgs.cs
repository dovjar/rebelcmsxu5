using System;
using Rebel.Framework.DependencyManagement;

namespace Rebel.Cms.Web.DependencyManagement.DemandBuilders
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