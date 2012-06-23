using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.EmbeddedViewEngine;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;
using RebelCms.Framework;
using RebelCms.Framework.Persistence;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Persistence.Model.Constants.AttributeDefinitions;
using RebelCms.Framework.Persistence.Model.Constants.Entities;
using RebelCms.Hive;
using RebelCms.Hive.ProviderGrouping;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Cms.Web.PropertyEditors.RebelCmsSystemEditors.UserGroupUsers
{
    [EmbeddedView("RebelCms.Cms.Web.PropertyEditors.RebelCmsSystemEditors.UserGroupUsers.Views.UserGroupUsersEditor.cshtml", "RebelCms.Cms.Web.PropertyEditors")]
    public class UserGroupUsersEditorModel : EditorModel
    {
        private string _userGroupName;
        private IRebelCmsApplicationContext _appContext;

        public UserGroupUsersEditorModel(string userGroupName, IRebelCmsApplicationContext appContext)
        {
            _userGroupName = userGroupName;
            _appContext = appContext;
        }

        public IEnumerable<SelectListItem> Value { get; set; }

        public override void SetModelValues(IDictionary<string, object> serializedVal)
        {
            var users = new List<SelectListItem>();

            if (!string.IsNullOrEmpty(_userGroupName))
            {
                using (var uow = _appContext.Hive.OpenReader<ISecurityStore>(new Uri("security://user-groups")))
                {
                    var userGroup = uow.Repositories.GetEntityByRelationType<UserGroup>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserGroupVirtualRoot)
                        .SingleOrDefault(x => x.Name.ToLower() == _userGroupName.ToLower());

                    var items =
                        uow.Repositories.GetLazyChildRelations(userGroup.Id, FixedRelationTypes.UserGroupRelationType)
                            .Select(x => (TypedEntity)x.Destination)
                            .OrderBy(x => x.Attribute<string>(NodeNameAttributeDefinition.AliasValue))
                            .ToArray();

                    users.AddRange(items.Select(item => new SelectListItem { Text = item.Attribute<string>(NodeNameAttributeDefinition.AliasValue), Value = item.Id.ToString() }));
                }
            }

            Value = users;
        }

        public override IDictionary<string, object> GetSerializedValue()
        {
            return new Dictionary<string, object>();
        }
    }
}
