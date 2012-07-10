using Rebel.Hive.ProviderGrouping;

namespace Rebel.Hive.ProviderSupport
{
    public interface IReadonlyProviderUnit : IUnit
    {
        AbstractReadonlyEntityRepository EntityRepository { get; }
    }
}