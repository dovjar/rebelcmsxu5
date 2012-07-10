namespace Rebel.Framework.Security.Model.Entities
{ 
    public interface IProfile : IMembershipUserId
    {
        HiveId Id { get; set; }
        string Name { get; set; }
    }
}
