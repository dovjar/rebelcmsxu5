using Rebel.Framework.EntityGraph.Domain;
using Rebel.Framework.EntityGraph.Domain.Brokers;
using Rebel.Framework.EntityGraph.Domain.EntityAdaptors;

namespace Prototyping.Rebel.Framework.Playground
{
    public class Article : Content
    {
        public Article()
        {
            // Basic setup
            this.Setup();

            // Get the DocType for this Content
            EntityType = new ArticleDocType();

            this.AsDynamic().textdata = "New text data";
        }
    }
}