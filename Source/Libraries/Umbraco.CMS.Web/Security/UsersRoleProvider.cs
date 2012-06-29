using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Security.Model.Entities;

namespace Umbraco.Cms.Web.Security
{
    public class UsersRoleProvider : AbstractUmbracoRoleProvider<User>
    {
        public override Uri HiveUri
        {
            get { return new Uri("security://user-groups");}
        }
    }
}
