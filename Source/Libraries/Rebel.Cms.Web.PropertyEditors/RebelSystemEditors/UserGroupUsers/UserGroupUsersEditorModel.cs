using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.EmbeddedViewEngine;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;
using Rebel.Framework;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Hive;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;
using FixedHiveIds = Rebel.Framework.Security.Model.FixedHiveIds;

namespace Rebel.Cms.Web.PropertyEditors.RebelSystemEditors.UserGroupUsers
{
    [EmbeddedView("Rebel.Cms.Web.PropertyEditors.RebelSystemEditors.UserGroupUsers.Views.UserGroupUsersEditor.cshtml", "Rebel.Cms.Web.PropertyEditors")]
    public class UserGroupUsersEditorModel : EditorModel
    {
        private string _userGroupName;
        private IRebelApplicationContext _appContext;

        public UserGroupUsersEditorModel(string userGroupName, IRebelApplicationContext appContext)
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
                    var userGroup = uow.Repositories.GetChildren<UserGroup>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserGroupVirtualRoot)
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
