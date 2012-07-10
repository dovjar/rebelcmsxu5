using Rebel.Cms.Web.Context;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model;

namespace Rebel.Cms.Web.PropertyEditors.NodeSelector
{
    /// <summary>
    /// The class that the dynamic start node query inherits from
    /// </summary>
    public abstract class AbstractStartNodeQuery
    {
        public abstract HiveId StartNodeId(IBackOfficeRequestContext requestContext, TypedEntity currentNode, TypedEntity rootNode);
    }
}