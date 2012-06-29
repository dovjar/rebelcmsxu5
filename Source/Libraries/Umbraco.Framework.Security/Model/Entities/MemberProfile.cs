using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Security.Model.Schemas;

namespace Umbraco.Framework.Security.Model.Entities
{
    public class MemberProfile : Profile
    {
        public MemberProfile()
        {
            this.SetupFromSchema<MasterMemberProfileSchema>();
        }
    }
}
