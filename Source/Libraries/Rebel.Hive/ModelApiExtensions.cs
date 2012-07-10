using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Framework.Persistence.Model.IO;

namespace Rebel.Hive
{
    using Rebel.Framework;
    using Rebel.Framework.Persistence;
    using Rebel.Framework.Persistence.Model;
    using Rebel.Framework.Persistence.Model.Attribution.MetaData;
    using Rebel.Framework.Persistence.Model.Constants;
    using Rebel.Framework.Persistence.Model.Versioning;
    using Rebel.Hive.ProviderGrouping;
    using Rebel.Hive.RepositoryTypes;

    public static class ModelApiExtensions
    {
        public static IBuilderStarter<TProviderFilter> UsingStore<TProviderFilter>(
            this IHiveManager hiveManager)
            where TProviderFilter : class, IProviderTypeFilter
        {
            return new BuilderStarter<TProviderFilter>(hiveManager);
        }

        public static T CreateSchema<T, TProviderFilter>(
            this IHiveManager hiveManager,
            string alias,
            string name = null)
            where T : EntitySchema, new()
            where TProviderFilter : class, IProviderTypeFilter
        {
            name = name ?? alias;
            var schema = new T { Alias = alias, Name = name };
            return schema;
        }

        public static IFileStoreStep<File, IFileStore> FileStore(
            this IHiveManager hiveManager)
        {
            return hiveManager.FileStore<IFileStore>();
        }

        public static IFileStoreStep<File, TProviderFilter> FileStore<TProviderFilter>(
            this IHiveManager hiveManager)
            where TProviderFilter : class, IProviderTypeFilter
        {
            return new FileStoreStep<File, TProviderFilter>(hiveManager);
        }

        public static IModelSaveResult<File> SaveFile<TProviderFilter>(
            this IFileStoreStep<File, TProviderFilter> fileStoreStep,
            string filename,
            string textContent)
            where TProviderFilter : class, IProviderTypeFilter
        {
            Mandate.ParameterNotNullOrEmpty(filename, "filename");
            var file = new File(filename, textContent);
            return fileStoreStep.SaveFile(file);
        }

        public static IModelSaveResult<File> SaveFile<TProviderFilter>(
            this IFileStoreStep<File, TProviderFilter> fileStoreStep,
            string filename,
            byte[] contentBytes)
            where TProviderFilter : class, IProviderTypeFilter
        {
            Mandate.ParameterNotNullOrEmpty(filename, "filename");
            var file = new File(filename, contentBytes);
            return fileStoreStep.SaveFile(file);
        }

        public static IModelSaveResult<File> CreateDirectory<TProviderFilter>(
            this IFileStoreStep<File, TProviderFilter> fileStoreStep,
            string containerName)
            where TProviderFilter : class, IProviderTypeFilter
        {
            Mandate.ParameterNotNullOrEmpty(containerName, "containerName");
            var file = new File {IsContainer = true, Name = containerName};
            return fileStoreStep.SaveFile(file);
        }

        public static IModelSaveResult<T> SaveFile<T, TProviderFilter>(
            this IFileStoreStep<File, TProviderFilter> fileStoreStep,
            T file)
            where T : File
            where TProviderFilter : class, IProviderTypeFilter
        {
            Mandate.ParameterNotNull(file, "file");
            using (var unit = fileStoreStep.HiveManager.OpenWriter<TProviderFilter>())
            {
                unit.Repositories.AddOrUpdate(file);
                unit.Complete();

                try
                {
                    unit.Repositories.AddOrUpdate(file);
                    unit.Complete();
                    return new ModelSaveResult<T>(true, file);
                }
                catch (Exception ex)
                {
                    unit.Abandon();
                    return new ModelSaveResult<T>(false, file, ex);
                }
            }
        }

        public static File GetFile<TProviderFilter>(
            this IFileStoreStep<File, TProviderFilter> fileStoreStep,
            string filename)
            where TProviderFilter : class, IProviderTypeFilter
        {
            Mandate.ParameterNotNullOrEmpty(filename, "filename");
            using (var unit = fileStoreStep.HiveManager.OpenReader<TProviderFilter>())
            {
                return unit.Repositories.Get<File>(HiveId.Parse(filename));
            }
        }

        public static IEnumerable<File> GetFiles<TProviderFilter>(
            this IFileStoreStep<File, TProviderFilter> fileStoreStep,
            string containerName,
            bool includeDescendants = false)
            where TProviderFilter : class, IProviderTypeFilter
        {
            Mandate.ParameterNotNullOrEmpty(containerName, "containerName");
            using (var unit = fileStoreStep.HiveManager.OpenReader<TProviderFilter>())
            {
                var container = unit.Repositories.Get<File>(HiveId.Parse(containerName));
                if (container == null)
                    throw new ArgumentOutOfRangeException(containerName, containerName, "Directory does not exist");
                if (includeDescendants)
                {
                    var descendantIds = unit.Repositories.GetDescendantIds(container.Id, FixedRelationTypes.DefaultRelationType);
                    return unit.Repositories.Get<File>(false, descendantIds).Where(x => !x.IsContainer);
                }
                var childIds = unit.Repositories.GetChildRelations(container.Id, FixedRelationTypes.DefaultRelationType).Select(x => x.DestinationId).ToArray();
                return unit.Repositories.Get<File>(false, childIds).Where(x => !x.IsContainer);
            }
        }

        public static ISchemaBuilderStep<EntitySchema, TProviderFilter> NewSchema<TProviderFilter>(
            this IBuilderStarter<TProviderFilter> builderStep,
            string alias,
            string name = null)
            where TProviderFilter : class, IProviderTypeFilter
        {
            return builderStep.HiveManager.NewSchema<TProviderFilter>(alias, name);
        }

        public static ISchemaBuilderStep<EntitySchema, TProviderFilter> NewSchema<TProviderFilter>(
            this IHiveManager hiveManager,
            string alias,
            string name = null)
            where TProviderFilter : class, IProviderTypeFilter
        {
            return hiveManager.NewSchema<EntitySchema, TProviderFilter>(alias, name);
        }

        public static ISchemaBuilderStep<T, TProviderFilter> NewSchema<T, TProviderFilter>(
            this IHiveManager hiveManager,
            string alias,
            string name = null)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            return new SchemaBuilderStep<T, TProviderFilter>(hiveManager, hiveManager.CreateSchema<T, TProviderFilter>(alias, name));
        }

        public static ISchemaBuilderStep<T, TProviderFilter> InheritFrom<T, TProviderFilter>(
            this ISchemaBuilderStep<T, TProviderFilter> builder,
            string schemaAlias)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            Mandate.ParameterNotNull(builder, "builder");
            Mandate.ParameterNotNullOrEmpty(schemaAlias, "schemaAlias");

            // Find the schema with the relevant alias
            var parent = builder.HiveManager.GetSchemaByAlias<TProviderFilter>(schemaAlias);
            if (parent == null)
                throw new ArgumentOutOfRangeException("An existing schema cannot be found with the alias '{0}'".InvariantFormat(schemaAlias));

            // Add a relation between the parent and the schema we're building
            builder.Item.RelationProxies.EnlistParent(parent, FixedRelationTypes.DefaultRelationType);

            return builder;
        }

        public static ISchemaBuilderStep<T, TProviderFilter> InheritFrom<T, TProviderFilter>(
            this ISchemaBuilderStep<T, TProviderFilter> builder,
            ISchemaBuilderStep<T, TProviderFilter> otherSchemaBuilder)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            Mandate.ParameterNotNull(builder, "builder");
            Mandate.ParameterNotNull(otherSchemaBuilder, "otherSchemaBuilder");
            Mandate.ParameterNotNull(otherSchemaBuilder.Item, "otherSchemaBuilder.Item");

            // Add a relation between the parent and the schema we're building
            builder.Item.RelationProxies.EnlistParent(otherSchemaBuilder.Item, FixedRelationTypes.DefaultRelationType);

            return builder;
        }

        public static ISchemaBuilderStep<T, TProviderFilter> SaveTo<T, TProviderFilter>(
            this ISchemaBuilderStep<T, TProviderFilter> builder,
            IGroupUnit<TProviderFilter> writer)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            Mandate.ParameterNotNull(builder, "builder");
            Mandate.ParameterNotNull(writer, "writer");
            Mandate.ParameterCondition(!writer.WasAbandoned, "writer.WasAbandoned");

            writer.Repositories.Schemas.AddOrUpdate(builder.Item);

            return builder;
        }

        public static ISchemaBuilderStep<T, TProviderFilter> IsOnlyInheritable<T, TProviderFilter>(
            this ISchemaBuilderStep<T, TProviderFilter> builder)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            return builder.SetCustom(x => x.IsAbstract = true);
        }

        public static ISchemaBuilderStep<T, TProviderFilter> SetCustom<T, TProviderFilter>(
            this ISchemaBuilderStep<T, TProviderFilter> builder,
            Action<T> customModifier)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            Mandate.ParameterNotNull(builder, "builder");

            customModifier.Invoke(builder.Item);

            return builder;
        }

        public static CompositeEntitySchema GetSchemaByAlias<TProviderFilter>(
            this IHiveManager hiveManager,
            string alias)
            where TProviderFilter : class, IProviderTypeFilter
        {
            using (var uow = hiveManager.OpenReader<TProviderFilter>())
            {
                var matching = uow.Repositories.Schemas.GetAll<EntitySchema>().FirstOrDefault(x => x.Alias.InvariantEquals(alias));
                if (matching == null) return null;
                return uow.Repositories.Schemas.GetComposite(matching);
            }
        }

        public static ISchemaBuilderStep<T, TProviderFilter> Define<T, TProviderFilter>(
            this ISchemaBuilderStep<T, TProviderFilter> builder,
            string propertyAlias,
            AttributeType type,
            AttributeGroup group)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            var def = new AttributeDefinition(propertyAlias, propertyAlias) { AttributeType = type, AttributeGroup = @group };
            builder.Item.AttributeDefinitions.Add(def);
            return builder;
        }

        public static ISchemaBuilderStep<T, TProviderFilter> Define<T, TProviderFilter>(
            this ISchemaBuilderStep<T, TProviderFilter> builder,
            string propertyAlias,
            string typeAlias,
            AttributeGroup group)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            return Define(builder, propertyAlias, type => type.UseExistingType(typeAlias), group);
        }

        public static ISchemaBuilderStep<T, TProviderFilter> Define<T, TProviderFilter>(
            this ISchemaBuilderStep<T, TProviderFilter> builder,
            string propertyAlias,
            string typeAlias,
            string groupAlias,
            string groupName = null)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            // Try to get the group from the existing builder's schema
            AttributeGroup group = null;
            if (builder.Item != null)
            {
                group = builder.Item.AttributeGroups.FirstOrDefault(x => x.Alias == groupAlias);
                if (group == null)
                {
                    // See if it's a composite schema and try to get an inherited group
                    var composite = builder.Item as CompositeEntitySchema;
                    if (composite != null)
                    {
                        group = composite.InheritedAttributeGroups.FirstOrDefault(x => x.Alias == groupAlias);
                    }
                }
            }
            if (group == null)
            {
                // Create a group
                groupName = groupName ?? groupAlias;
                group = new AttributeGroup(groupAlias, groupName, 0);
            }

            return Define(builder, propertyAlias, type => type.UseExistingType(typeAlias), group);
        }



        public static ISchemaBuilderStep<T, TProviderFilter> Define<T, TProviderFilter>(
            this ISchemaBuilderStep<T, TProviderFilter> builder,
            string propertyAlias,
            Func<IEntityBuilderStep<AttributeType, TProviderFilter>, ISchemaBuilderStep<AttributeType, TProviderFilter>> typeBuilder,
            AttributeGroup group)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            var type = typeBuilder.Invoke(new EntityBuilderStarter<AttributeType, TProviderFilter>(builder.HiveManager));
            return Define(builder, propertyAlias, type.Item, group);
        }

        public static ISchemaSaveResult<T> Commit<T, TProviderFilter>(this ISchemaBuilderStep<T, TProviderFilter> modelBuilder)
            where TProviderFilter : class, IProviderTypeFilter
            where T : AbstractSchemaPart, new()
        {
            using (var unit = modelBuilder.HiveManager.OpenWriter<TProviderFilter>())
            {
                var item = modelBuilder.Item;
                try
                {
                    unit.Repositories.Schemas.AddOrUpdate(item);
                    unit.Complete();
                    return new SchemaSaveResult<T>(true, item);
                }
                catch (Exception ex)
                {
                    unit.Abandon();
                    return new SchemaSaveResult<T>(false, item, ex);
                }
            }
        }


    }

    public static class ModelTypeApiExtensions
    {
        public static ISchemaBuilderStep<AttributeType, TProviderFilter> UseExistingType<TProviderFilter>(this IEntityBuilderStep<AttributeType, TProviderFilter> builder, string alias)
            where TProviderFilter : class, IProviderTypeFilter
        {
            var registry = AttributeTypeRegistry.Current;
            return UseExistingType(builder, registry, alias);
        }

        public static ISchemaBuilderStep<AttributeType, TProviderFilter> UseExistingType<TProviderFilter>(this IEntityBuilderStep<AttributeType, TProviderFilter> builder, IAttributeTypeRegistry registry, string alias)
            where TProviderFilter : class, IProviderTypeFilter
        {
            var check = registry.TryGetAttributeType(alias);
            if (!check.Success)
            {
                // Get all the registered types to provide a helpful message
                var allAliases = registry.GetAllRegisteredAliases();
                throw new InvalidOperationException(
                    "AttributeType '{0}' is not registered with the supplied IAttributeTypeRegistry. Available types are:\n{1}"
                        .InvariantFormat(alias, string.Join(@"\n", allAliases)));
            }
            var existing = check.Result;
            return new SchemaBuilderStep<AttributeType, TProviderFilter>(builder.HiveManager, existing);
        }
    }

    public interface IBuilderStep<out TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
    {
        IHiveManager HiveManager { get; }
    }

    public interface IBuilderStarter<out TProviderFilter> : IBuilderStep<TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
    {
    }

    public interface IEntityBuilderStep<T, out TProviderFilter> : IBuilderStep<TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
        where T : AbstractEntity
    {
    }

    public interface ISchemaBuilderStep<T, out TProviderFilter> : IModelBuilderStep<T, TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
        where T : AbstractSchemaPart
    {
    }

    public interface IFileStoreStep<T, out TProviderFilter> : IBuilderStep<TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
        where T : File
    {

    }

    public interface IModelBuilderStep<T, out TProviderFilter> : IEntityBuilderStep<T, TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
        where T : AbstractEntity
    {
        T Item { get; }
    }

    public interface IRevisionBuilderStep<T, out TProviderFilter> : IEntityBuilderStep<T, TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
        where T : TypedEntity, IVersionableEntity
    {
        T Item { get; }
        Revision<T> Revision { get; }
    }

    public class BuilderStarter<TProviderFilter> : IBuilderStarter<TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
    {
        public BuilderStarter(IHiveManager hiveManager)
        {
            HiveManager = hiveManager;
        }

        public IHiveManager HiveManager { get; protected set; }
    }

    public class EntityBuilderStarter<T, TProviderFilter> : BuilderStarter<TProviderFilter>, IEntityBuilderStep<T, TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
        where T : AbstractEntity
    {
        public EntityBuilderStarter(IHiveManager hiveManager)
            : base(hiveManager)
        { }
    }

    public class FileStoreStep<T, TProviderFilter> : IFileStoreStep<T, TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
        where T : File
    {
        public IHiveManager HiveManager { get; protected set; }

        public FileStoreStep(IHiveManager hiveManager)
        {
            HiveManager = hiveManager;
        }
    }

    public class SchemaBuilderStep<T, TProviderFilter> : ISchemaBuilderStep<T, TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
        where T : AbstractSchemaPart
    {
        public IHiveManager HiveManager { get; protected set; }

        public SchemaBuilderStep(IHiveManager hiveManager, T item)
        {
            HiveManager = hiveManager;
            Item = item;
        }

        public T Item { get; protected set; }
    }

    public interface ISaveResult
    {
        bool Success { get; }
        IEnumerable<Exception> Errors { get; }
    }

    public interface IModelSaveResult<out T> : ISaveResult
        where T : TypedEntity
    {
        T Item { get; }
    }

    public interface IRevisionSaveResult<T> : IModelSaveResult<T>
        where T : TypedEntity
    {
        IRevision<T> Revision { get; }
    }

    public interface ISchemaSaveResult<out T> : ISaveResult
        where T : AbstractSchemaPart
    {
        T Item { get; }
    }

    public class ModelSaveResult<T> : IModelSaveResult<T>
        where T : TypedEntity
    {
        public ModelSaveResult(bool success, T item, params Exception[] errors)
        {
            Success = success;
            Item = item;
            Errors = errors;
        }

        public bool Success { get; protected set; }

        public virtual T Item { get; protected set; }

        public IEnumerable<Exception> Errors { get; protected set; }
    }

    public class RevisionSaveResult<T> : ModelSaveResult<T>, IRevisionSaveResult<T>
        where T : TypedEntity
    {
        public RevisionSaveResult(bool success, IRevision<T> item, params Exception[] errors)
            : base(success, item.Item, errors)
        {
            Revision = item;
        }

        #region Implementation of IRevisionSaveResult<T>

        public IRevision<T> Revision { get; protected set; }

        #endregion

        public override T Item
        {
            get
            {
                return Revision == null ? null : Revision.Item;
            }
            protected set
            {
                if (Revision == null)
                    Revision = new Revision<T>(value);
                else
                {
                    var casted = Revision as Revision<T>;
                    if (casted != null)
                        casted.Item = value;
                    else Revision = new Revision<T>(value);
                }
            }
        }
    }

    public class SchemaSaveResult<T> : ISchemaSaveResult<T>
        where T : AbstractSchemaPart
    {
        public SchemaSaveResult(bool success, T item, params Exception[] errors)
        {
            Success = success;
            Item = item;
            Errors = errors;
        }

        public bool Success { get; protected set; }

        public T Item { get; protected set; }

        public IEnumerable<Exception> Errors { get; protected set; }
    }
}
