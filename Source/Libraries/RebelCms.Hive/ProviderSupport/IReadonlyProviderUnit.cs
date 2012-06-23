using RebelCms.Hive.ProviderGrouping;

namespace RebelCms.Hive.ProviderSupport
{
    public interface IReadonlyProviderUnit : IUnit
    {
        AbstractReadonlyEntityRepository EntityRepository { get; }
    }
}