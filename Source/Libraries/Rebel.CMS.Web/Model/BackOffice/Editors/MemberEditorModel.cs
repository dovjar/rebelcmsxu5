using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Rebel.Cms.Web.Context;
using Rebel.Framework;
using System.ComponentModel.DataAnnotations;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Security;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Model.BackOffice.Editors
{
    [Bind(Exclude = "LastLoginDate,LastActivityDate,LastPasswordChangeDate")]
    public class MemberEditorModel : MembershipUserEditorModel
    {
        public MemberEditorModel()
        {
            ConfirmPassword = Password;
        }

        public override HiveId ParentId { get; set; }
    }
}
