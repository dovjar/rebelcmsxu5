using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Cms.Web.Context;
using Rebel.Framework;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Tasks;
using Rebel.Hive;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Tasks
{
    public class PublishNodeTask : ConfigurationTask
    {
        public PublishNodeTask(ConfigurationTaskContext configurationTaskContext) 
            : base(configurationTaskContext)
        { }

        public override void Execute(TaskExecutionContext context)
        {
            if (!ConfigurationTaskContext.Parameters.ContainsKey("id"))
                throw new ArgumentException("No id parameter supplied");

            var nodeId = HiveId.Parse(ConfigurationTaskContext.Parameters["id"]);
            if(nodeId.IsNullValueOrEmpty())
                throw new ArgumentException("The id parameter is not a valid HiveId");

            var includeChildren = ConfigurationTaskContext.Parameters.ContainsKey("includeChildren") &&
                ConfigurationTaskContext.Parameters["includeChildren"].InvariantEquals("true");

            using (var uow = ApplicationContext.Hive.OpenWriter<IContentStore>())
            {
                var contentEntity = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(nodeId);
                if (contentEntity == null)
                    throw new ArgumentException(string.Format("No entity found for id: {0}", nodeId));

                //get its children recursively
                if (includeChildren)
                {
                    // Get all descendents
                    var descendents = uow.Repositories.GetDescendentRelations(nodeId, FixedRelationTypes.DefaultRelationType);

                    foreach (var descendent in descendents)
                    {
                        //get the revision 
                        var revisionEntity =
                            uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(descendent.DestinationId);

                        //publish it if it's already published or if the user has specified to publish unpublished content
                        var publishRevision = revisionEntity.CopyToNewRevision(FixedStatusTypes.Published);
                        
                        uow.Repositories.Revisions.AddOrUpdate(publishRevision);
                    }
                }

                //publish this node
                var toPublish = contentEntity.CopyToNewRevision(FixedStatusTypes.Published);
                uow.Repositories.Revisions.AddOrUpdate(toPublish);

                //save
                uow.Complete();
            }
        }
    }
}
