using System;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.IO;
using Rebel.Cms.Web.Mapping;
using Rebel.Cms.Web.Model;
using Rebel.Cms.Web.System;

using Rebel.Framework;
using Rebel.Framework.DependencyManagement;
using Rebel.Framework.Security.Mapping;
using Rebel.Framework.TypeMapping;

namespace Rebel.Cms.Web.DependencyManagement.DemandBuilders
{
    /// <summary>
    /// registers all model mappers and resolvers into the container
    /// </summary>
    public class ModelMappingsDemandBuilder : IDependencyDemandBuilder
    {
        public void Build(IContainerBuilder containerBuilder, IBuilderContext context)
        {
            containerBuilder.For<MapResolverContext>().KnownAsSelf().ScopedAs.Singleton();

            // register the model mappers
            containerBuilder.For<RenderTypesModelMapper>()
                .KnownAs<AbstractMappingEngine>()
                .WithMetadata<TypeMapperMetadata, bool>(x => x.MetadataGeneratedByMapper, true)
                .ScopedAs.Singleton();

            containerBuilder
                .For<FrameworkModelMapper>()
                .KnownAs<AbstractMappingEngine>()
                .WithMetadata<TypeMapperMetadata, bool>(x => x.MetadataGeneratedByMapper, true)
                .ScopedAs.Singleton();

            //register model mapper for security model objects
            containerBuilder
                .For<SecurityModelMapper>()
                .KnownAs<AbstractMappingEngine>()
                .WithMetadata<TypeMapperMetadata, bool>(x => x.MetadataGeneratedByMapper, true)
                .ScopedAs.Singleton();

            //register model mapper for web model objects
            containerBuilder
                .For<CmsModelMapper>()
                .KnownAs<AbstractMappingEngine>()
                .WithMetadata<TypeMapperMetadata, bool>(x => x.MetadataGeneratedByMapper, true)
                .ScopedAs.Singleton();
        }
    }
}