namespace RebelCms.Framework.Linq
{
    using System.Linq;

    using RebelCms.Framework.Linq.ResultBinding;

    public interface IQueryContext<out T>
    {
        //T RenderItem { get; set; }
        IQueryableDataSource QueryableDataSource { get; }
        IQueryable<T> Query();
        IQueryable<TSpecific> Query<TSpecific>();
        IQueryable<TSpecific> Query<TSpecific>(ObjectBinder objectBinder);
    }
}