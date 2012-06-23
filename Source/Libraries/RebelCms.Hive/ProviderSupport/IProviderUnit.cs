using RebelCms.Hive.ProviderGrouping;

namespace RebelCms.Hive.ProviderSupport
{
    public interface IProviderUnit : IUnit
    {
        AbstractEntityRepository EntityRepository { get; }
    }
}