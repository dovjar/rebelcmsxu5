using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;

using Rebel.Hive;
using Rebel.Hive.RepositoryTypes;
using FixedHiveIds = Rebel.Framework.Security.Model.FixedHiveIds;

namespace Rebel.Cms.Web.PropertyEditors.ListPicker.DataSources
{
    public class UserGroupsListPickerDataSource : IListPickerDataSource
    {
        public IDictionary<string, string> GetData()
        {
            var data = new Dictionary<string, string>();

            var context = DependencyResolver.Current.GetService<IRebelApplicationContext>();
            using (var uow = context.Hive.OpenReader<ISecurityStore>(new Uri("security://user-groups")))
            {
                var items = uow.Repositories.GetChildren<TypedEntity>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserGroupVirtualRoot)
                    .OrderBy(x => x.Attributes[NodeNameAttributeDefinition.AliasValue].DynamicValue)
                    .ToArray();

                foreach (var typedEntity in items)
                {
                    data.Add(typedEntity.Attributes[NodeNameAttributeDefinition.AliasValue].DynamicValue, typedEntity.Attributes[NodeNameAttributeDefinition.AliasValue].DynamicValue);
                }
            }

            return data;
        }
    }
}
