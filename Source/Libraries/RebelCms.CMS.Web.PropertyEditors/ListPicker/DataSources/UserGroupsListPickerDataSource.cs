using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Persistence.Model.Constants.AttributeDefinitions;

using RebelCms.Hive;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Cms.Web.PropertyEditors.ListPicker.DataSources
{
    public class UserGroupsListPickerDataSource : IListPickerDataSource
    {
        public IDictionary<string, string> GetData()
        {
            var data = new Dictionary<string, string>();

            var context = DependencyResolver.Current.GetService<IRebelCmsApplicationContext>();
            using (var uow = context.Hive.OpenReader<ISecurityStore>(new Uri("security://user-groups")))
            {
                var items = uow.Repositories.GetEntityByRelationType<TypedEntity>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserGroupVirtualRoot)
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
