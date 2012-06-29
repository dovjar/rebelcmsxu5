using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Security.Model.Entities;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Framework.Persistence;
using FixedHiveIds = Umbraco.Framework.Security.Model.FixedHiveIds;

namespace Umbraco.Cms.Web
{
    public static class MemberExtensions
    {
        /// <summary>
        /// Sets up a Member by Member Type alias.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="alias">The alias.</param>
        public static void SetupFromSchema(this Member member, string alias)
        {
            var hive = DependencyResolver.Current.GetService<IHiveManager>();
            using (var uow = hive.OpenReader<ISecurityStore>())
            {
                member.SetupFromSchema(alias, uow);
            }
        }

        /// <summary>`
        /// Sets up a Member by Member Type alias.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="uow">The uow.</param>
        public static void SetupFromSchema(this Member member, string alias, IReadonlyGroupUnit<ISecurityStore> uow)
        {
            var memberGroupRelations = uow.Repositories.Schemas
                .GetDescendentRelations(FixedHiveIds.MasterMemberProfileSchema, FixedRelationTypes.DefaultRelationType);
            var memberTypes = uow.Repositories.Schemas
                .Get<EntitySchema>(true, memberGroupRelations.Select(x => x.DestinationId).ToArray());
            var memberType = memberTypes.SingleOrDefault(x => x.Alias.Equals(alias, StringComparison.InvariantCultureIgnoreCase));

            if (memberType == null)
                throw new ApplicationException(string.Format("No member type found with the alias '{0}'", alias));

            var compositeMemberType = uow.Repositories.Schemas.GetComposite(memberType);

            member.SetupFromSchema(compositeMemberType);
        }

        /// <summary>
        /// Sets up a Member by Member Type alias.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="uow">The uow.</param>
        public static void SetupFromSchema(this Member member, string alias, IGroupUnit<ISecurityStore> uow)
        {
            var memberGroupRelations = uow.Repositories.Schemas
                .GetDescendentRelations(Framework.Security.Model.FixedHiveIds.MasterMemberProfileSchema, FixedRelationTypes.DefaultRelationType);
            var memberTypes = uow.Repositories.Schemas
                .Get<EntitySchema>(true, memberGroupRelations.Select(x => x.DestinationId).ToArray());
            var memberType = memberTypes.SingleOrDefault(x => x.Alias.Equals(alias, StringComparison.InvariantCultureIgnoreCase));

            if (memberType == null)
                throw new ApplicationException(string.Format("No member type found with the alias '{0}'", alias));

            var compositeMemberType = uow.Repositories.Schemas.GetComposite(memberType);

            member.SetupFromSchema(compositeMemberType);
        }
    }
}
