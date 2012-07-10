using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Security.Model.Schemas;

namespace Rebel.Framework.Security.Model.Entities
{
    public class MemberProfile : Profile
    {
        public MemberProfile()
        {
            this.SetupFromSchema<MasterMemberProfileSchema>();
        }
    }
}
