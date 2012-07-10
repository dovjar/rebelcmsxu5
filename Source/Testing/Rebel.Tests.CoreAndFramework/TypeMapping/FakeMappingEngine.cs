using System;
using Rebel.Framework.Context;
using Rebel.Framework.TypeMapping;

namespace Rebel.Tests.CoreAndFramework.TypeMapping
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