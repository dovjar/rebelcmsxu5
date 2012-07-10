using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Framework.Persistence.Model;

namespace Rebel.Cms.Web.PropertyEditors.NodeSelector
{
    /// <summary>
    /// The class that the dynamic filter inherits from
    /// </summary>
    public abstract class AbstractNodeFilter
    {
        public abstract bool IsMatch(IBackOfficeRequestContext requestContext, TypedEntity entity);
    }
}