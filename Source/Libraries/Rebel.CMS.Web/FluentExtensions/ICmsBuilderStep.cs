using Rebel.Framework.Security;
using Rebel.Framework.Security.Model.Entities;

namespace Rebel.Cms.Web.FluentExtensions
{
    using Rebel.Hive;
    using Rebel.Hive.RepositoryTypes;

    public interface ICmsBuilderStep<out TProviderFilter> : IBuilderStep<TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
    {
        
    }
}