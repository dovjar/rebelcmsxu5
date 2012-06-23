using AutoMapper;
using RebelCms.Cms.Domain.BackOffice.Editors;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Associations;

namespace RebelCms.Cms.Domain.Mapping
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