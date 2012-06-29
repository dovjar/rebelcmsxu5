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
using Umbraco.Framework.Security.Model.Schemas;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    [Bind(Exclude = "LastLoginDate,LastActivityDate,LastPasswordChangeDate")]
    public class UserEditorModel : MembershipUserEditorModel
    {
        public UserEditorModel()
        {
            ConfirmPassword = Password;
        }

        public override HiveId ParentId { get; set; }

        [Range(5, int.MaxValue)]
        [Required]
        public int SessionTimeout
        {
            get { return GetPropertyEditorModelValue(MasterUserProfileSchema.SessionTimeoutAlias, x => x.ValueAsInteger) ?? 60; }
            set { SetPropertyEditorModelValue(MasterUserProfileSchema.SessionTimeoutAlias, x => x.ValueAsInteger = value); }
        }

        public HiveId StartContentHiveId
        {
            get { return GetPropertyEditorModelValue(MasterUserProfileSchema.StartContentHiveIdAlias, x => x.Value) ?? HiveId.Empty; }
            set { SetPropertyEditorModelValue(MasterUserProfileSchema.StartContentHiveIdAlias, x => x.Value = value); }
        }

        public HiveId StartMediaHiveId
        {
            get { return GetPropertyEditorModelValue(MasterUserProfileSchema.StartMediaHiveIdAlias, x => x.Value) ?? HiveId.Empty; }
            set { SetPropertyEditorModelValue(MasterUserProfileSchema.StartMediaHiveIdAlias, x => x.Value = value); }
        }

        public IEnumerable<string> Applications
        {
            get { return GetPropertyEditorModelValue(MasterUserProfileSchema.ApplicationsAlias, x => x.Value); }
            set { SetPropertyEditorModelValue(MasterUserProfileSchema.ApplicationsAlias, x => x.Value = value); }
        }
    }
}
