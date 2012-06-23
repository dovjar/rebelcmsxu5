using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCmsFramework.EntityGraph.Domain.Abstracts;

namespace RebelCmsFramework.EntityGraph.Synonyms
{
    public class ContentGraph : EntitySynonymGraph<IContentEntitySynonym>
    {
        public ContentGraph(IList<Content> incoming)
            : base(incoming.Cast<IContentEntitySynonym>().ToList())
        {

        }

    }
}
