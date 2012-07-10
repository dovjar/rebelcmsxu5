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
using Rebel.Framework.Security.Model.Schemas;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Model.BackOffice.Editors
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
