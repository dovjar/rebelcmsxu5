using System;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.IO;
using RebelCms.Cms.Web.Mapping;
using RebelCms.Cms.Web.Model;
using RebelCms.Cms.Web.System;

using RebelCms.Framework;
using RebelCms.Framework.DependencyManagement;
using RebelCms.Framework.TypeMapping;

namespace RebelCms.Cms.Web.DependencyManagement.DemandBuilders
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

            //register model mapper for web model objects
            containerBuilder
                .For<CmsModelMapper>()
                .KnownAs<AbstractMappingEngine>()
                .WithMetadata<TypeMapperMetadata, bool>(x => x.MetadataGeneratedByMapper, true)
                .ScopedAs.Singleton();

            //register model mapper for membership provider models
            containerBuilder
                .For<MembershipModelMapper>()
                .KnownAs<AbstractMappingEngine>()
                .WithMetadata<TypeMapperMetadata, bool>(x => x.MetadataGeneratedByMapper, true)
                .ScopedAs.Singleton();
        }
    }
}