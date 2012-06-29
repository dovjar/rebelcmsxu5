using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Security.Model;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web
{
    public class UpgradeHelper
    {
        public static bool UsersUpgraded()
        {
            var hiveManager = DependencyResolver.Current.GetService<IHiveManager>();

            using (var coreUow = hiveManager.OpenWriter<IContentStore>())
            {
                var ids = new[]
                {
                    FixedHiveIds.UserProfileVirtualRoot, FixedHiveIds.MemberVirtualRoot,
                    FixedHiveIds.MemberProfileVirtualRoot, FixedHiveIds.MemberGroupVirtualRoot
                };

                return coreUow.Repositories.Get<TypedEntity>(true, ids).Count() == 4 &&
                    coreUow.Repositories.Schemas.Get<EntitySchema>(FixedHiveIds.RTMUserSchema) == null;
            }
        }

        public static bool ImageSchemaUpgraded()
        {
            var hiveManager = DependencyResolver.Current.GetService<IHiveManager>();

            using (var coreUow = hiveManager.OpenWriter<IContentStore>())
            {
                var imageSchema = coreUow.Repositories.Schemas.Get<EntitySchema>(Framework.Persistence.Model.Constants.FixedHiveIds.MediaImageSchema);
                return imageSchema.AttributeDefinitions[MediaImageSchema.UploadFileAlias].AttributeType.Alias == "uploader";
            }
        }
    }
}
