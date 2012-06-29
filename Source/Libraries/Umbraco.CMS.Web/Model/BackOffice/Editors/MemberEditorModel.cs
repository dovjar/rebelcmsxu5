using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Cms.Web.Context;
using Umbraco.Framework;
using System.ComponentModel.DataAnnotations;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Security;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
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
