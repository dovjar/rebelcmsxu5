using System;
using System.Collections.Generic;
using RebelCms.Framework.TypeMapping;

namespace RebelCms.Tests.Extensions
{
    public class FakeTypeMapperCollection : MappingEngineCollection
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref = "T:System.Object" /> class.
        /// </summary>
        public FakeTypeMapperCollection(AbstractMappingEngine[] dynamicTypeMaps)
            : base(new List<Lazy<AbstractMappingEngine, TypeMapperMetadata>>())
        {
            Array.ForEach(dynamicTypeMaps, x => Add(new Lazy<AbstractMappingEngine, TypeMapperMetadata>(() => x, new TypeMapperMetadata(true))));
        }
    }
}