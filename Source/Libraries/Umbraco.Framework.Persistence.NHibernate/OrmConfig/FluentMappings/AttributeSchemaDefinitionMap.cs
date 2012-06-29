using FluentNHibernate.Mapping;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings
{
    /// <summary>Represents the mapping of the 'AttributeSchemaDefinition' entity, represented by the 'AttributeSchemaDefinition' class.</summary>
    public partial class AttributeSchemaDefinitionMap : ClassMap<AttributeSchemaDefinition>
    {
        /// <summary>Initializes a new instance of the <see cref="AttributeSchemaDefinitionMap"/> class.</summary>
        public AttributeSchemaDefinitionMap()
        {
            OptimisticLock.None();

            Id(x => x.Id)
                .Access.CamelCaseField(Prefix.Underscore)
                .Column("NodeId")
                .GeneratedBy.Foreign("Node")
                .Not.Nullable();

            //HasOne(x => x.Node)
            //    .Cascade.All()
            //    .Access.CamelCaseField(Prefix.Underscore)
            //    .ForeignKey(this.GenerateFkName(x => x.Node))
            //    .Fetch.Join()
            //    .LazyLoad(Laziness.False);

            References(x => x.Node)
                .Access.Property()
                .Cascade.All()
                .Column("NodeId")
                .ReadOnly()
                .Fetch.Join()
                .LazyLoad(Laziness.False)
                .Not.Nullable().NotFound.Ignore()
                .ForeignKey(this.GenerateFkName(x => x.Node));

            Map(x => x.Alias)
               .Access.Property()
               .Not.Nullable()
               .Length(64)
               .Index(this.GenerateIndexName(x => x.Alias));

            //Map(x => x.Description).CustomType("StringClob").Access.CamelCaseField(Prefix.Underscore);

            Map(x => x.Name).Access.CamelCaseField(Prefix.Underscore);

            Map(x => x.SchemaType)
                .Access.CamelCaseField(Prefix.Underscore)
                .Length(100)
                .Not.Nullable()
                .Index(this.GenerateIndexName(x => x.SchemaType));

            Map(x => x.XmlConfiguration)
                .CustomType("StringClob")
                .Access.CamelCaseField(Prefix.Underscore);

            HasMany(x => x.AttributeDefinitions)
                .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.MergeSaveAllDeleteOrphan()
                .Fetch.Select().BatchSize(10)
                .AsSet()
                .Inverse()
                //.LazyLoad()
                .Key(x =>
                    {
                        x.Column("AttributeSchemaDefinitionId");
                        x.ForeignKey(this.GenerateFkName(y => y.AttributeDefinitions));
                    });

            HasMany(x => x.AttributeDefinitionGroups)
                .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.MergeSaveAllDeleteOrphan()
                .Fetch.Select().BatchSize(10)
                .AsSet()
                .Inverse()
                .LazyLoad()
                .Key(x =>
                {
                    x.Column("AttributeSchemaDefinitionId");
                    x.ForeignKey(this.GenerateFkName(y => y.AttributeDefinitionGroups));
                });

            //NOTE: added this mapping so that when you delete a schema all of the Nodes that reference the Schema as their scema. SD.
            HasMany(x => x.ReferencedNodes)
                .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Delete()
                .Fetch.Select().LazyLoad()
                .AsSet()
                .Inverse()
                .Key(x =>
                {
                    x.Column("AttributeSchemaDefinition_id");
                    x.ForeignKey(this.GenerateFkName(y => y.ReferencedNodes));
                });

            //Key(x =>
            //{
            //    x.Column("NodeId");
            //    x.ForeignKey(this.GenerateFkName<AttributeSchemaDefinition, Node>());
            //});


            AdditionalMappingInfo();
        }

        /// <summary>Partial method for adding additional mapping information in a partial class./ </summary>
        partial void AdditionalMappingInfo();
    }
}