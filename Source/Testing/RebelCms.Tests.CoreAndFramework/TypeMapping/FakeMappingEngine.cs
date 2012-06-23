using System;
using RebelCms.Framework.Context;
using RebelCms.Framework.TypeMapping;

namespace RebelCms.Tests.CoreAndFramework.TypeMapping
{
    public class FakeMappingEngine : AbstractFluentMappingEngine
    {
        public FakeMappingEngine(IFrameworkContext frameworkContext)
            : base(frameworkContext)
        {
        }

        public override void ConfigureMappings()
        {            
        }
    }
}