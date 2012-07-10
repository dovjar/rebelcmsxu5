using AutoMapper;
using Rebel.Cms.Domain.BackOffice.Editors;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Associations;

namespace Rebel.Cms.Domain.Mapping
{
    internal class ContentRelationResolver : IValueResolver
    {
        #region Implementation of IValueResolver

        public ResolutionResult Resolve(ResolutionResult source)
        {
            var relationSourceEntity = source.Context.DestinationValue as TypedPersistenceEntity;
            var relationDestinationNode = source.Context.SourceValue as EntityUIModel;
            if (relationSourceEntity == null && relationDestinationNode == null) return source;

            var relationCollection = new EntityRelationCollection(relationSourceEntity);
            relationCollection.Add(new RelationById(new ContentTreeRelationType(), relationSourceEntity.Id,
                                                    relationDestinationNode.ParentId));

            return source;
        }

        #endregion
    }
}