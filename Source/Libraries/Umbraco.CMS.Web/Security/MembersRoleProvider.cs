using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Security.Model.Entities;

namespace Umbraco.Cms.Web.Security
{
    public class MembersRoleProvider : AbstractUmbracoRoleProvider<Member>
    {
        public override Uri HiveUri
        {
            get { return new Uri("security://member-groups");}
        }
    }
}
