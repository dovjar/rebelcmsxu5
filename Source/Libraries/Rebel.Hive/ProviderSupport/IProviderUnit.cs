using Rebel.Hive.ProviderGrouping;

namespace Rebel.Hive.ProviderSupport
{
    public interface IProviderUnit : IUnit
    {
        AbstractEntityRepository EntityRepository { get; }
    }
}