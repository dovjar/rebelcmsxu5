using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Security.Model.Entities;

namespace Rebel.Cms.Web.Security
{
    public class UsersRoleProvider : AbstractRebelRoleProvider<User>
    {
        public override Uri HiveUri
        {
            get { return new Uri("security://user-groups");}
        }
    }
}
