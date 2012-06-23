using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;
using RebelCms.Framework;

namespace RebelCms.Cms.Web.PropertyEditors.RebelCmsSystemEditors.UserGroupUsers
{
    [RebelCmsPropertyEditor]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [PropertyEditor(CorePluginConstants.UserGroupUsersPropertyEditorId, "UserGroupUsers", "User Group Users")]
    public class UserGroupUsersEditor : ContentAwarePropertyEditor<UserGroupUsersEditorModel>
    {
        private readonly IBackOfficeRequestContext _backOfficeRequestContext;

        public UserGroupUsersEditor(IBackOfficeRequestContext backOfficeRequestContext)
        {
            Mandate.ParameterNotNull(backOfficeRequestContext, "backOfficeRequestContext");

            _backOfficeRequestContext = backOfficeRequestContext;
        }

        public override UserGroupUsersEditorModel CreateEditorModel()
        {
            return new UserGroupUsersEditorModel(IsContentModelAvailable ? GetContentModelValue<UserGroupEditorModel, string>(x => x.Name, string.Empty) : string.Empty, _backOfficeRequestContext.Application);
        }
    }
}
