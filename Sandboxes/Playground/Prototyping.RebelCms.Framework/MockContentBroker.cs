using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCmsFramework;
using RebelCmsFramework.EntityGraph.DataPersistence;
using RebelCmsFramework.EntityGraph.Domain;
using RebelCmsFramework.EntityGraph.Domain.Entity.Graph;
using RebelCmsFramework.EntityGraph.Domain.ObjectModel;
using RebelCmsFramework.EntityGraph.Synonyms;

namespace Prototyping.RebelCmsFramework
{
    public class MockContentBroker : IContentResolver
    {
        #region Implementation of IContentResolver

        public IEntitySynonymVertex<IContentVertex> Get(IMappedIdentifier identifier)
        {
            throw new NotImplementedException();
        }

        public EntitySynonymGraph<IContentVertex> Get(IEnumerable<IMappedIdentifier> identifiers)
        {
            throw new NotImplementedException();
        }

        public EntitySynonymGraph<IContentVertex> GetAll()
        {
            throw new NotImplementedException();
        }

        public IEntitySynonymVertex<IContentVertex> Get(IMappedIdentifier identifier, int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public EntitySynonymGraph<IContentVertex> Get(IEnumerable<IMappedIdentifier> identifiers, int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public EntitySynonymGraph<IContentVertex> GetAll(int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public IEntityRepositoryReader RepositoryReader { get; set; }
        public IEntityRepositoryWriter RepositoryWriter { get; set; }

        #endregion
    }
}
