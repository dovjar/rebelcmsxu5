using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model;
using Rebel.Framework;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Tasks;
using Rebel.Hive;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Tasks
{
    using Rebel.Framework.Context;
    using Rebel.Framework.Diagnostics;
    using Rebel.Framework.Persistence.Model.Attribution;
    using Rebel.Framework.Persistence.Model.Constants;
    using Rebel.Framework.Persistence.Model.Versioning;

    [Task("D54C8A93-F073-4D0F-B19C-2C4860273588", TaskTriggers.Hive.Revisions.PreAddOrUpdateOnUnitComplete, ContinueOnFailure = false)]
    public class EnsureUniqueNameTask : AbstractTask
    {
        public EnsureUniqueNameTask(IFrameworkContext frameworkContext)
            : base(frameworkContext)
        { }

        public override void Execute(TaskExecutionContext context)
        {
            var uow = context.EventSource as IGroupUnit<IProviderTypeFilter>;
            var args = context.EventArgs.CallerEventArgs as HiveRevisionPreActionEventArgs;
            if (uow != null && args != null)
            {
                var item = args.Entity.Item as TypedEntity;
                if (item != null && item.Attributes.Any(x => x.AttributeDefinition.Alias == NodeNameAttributeDefinition.AliasValue))
                {
                    //var siblings = uow.Repositories.GetBranchRelations(item.Id, FixedRelationTypes.DefaultRelationType)
                    //    .Select(x => x.DestinationId).ToArray();

                    //if(siblings.Length == 0)
                    //{
                    //    // Could be that it isn't saved yet
                    //    var parentsManual = item.RelationProxies.GetManualParentProxies().ToArray();
                    //    var firstManualParent = parentsManual.FirstOrDefault();
                    //    if (firstManualParent != null && !firstManualParent.Item.SourceId.IsNullValueOrEmpty())
                    //    {
                    //        var manualSiblings = uow.Repositories.GetChildRelations(firstManualParent.Item.SourceId).ToArray();
                    //        siblings = manualSiblings.Select(x => x.DestinationId).ToArray();
                    //    }
                    //}

                    //if (siblings.Length == 0)
                    //    return;

                    // Get the parent(s)
                    var parentIds = uow.Repositories.GetParentRelations(item.Id).Select(x => x.SourceId).ToArray();
                    if (!parentIds.Any())
                    {
                        parentIds = item.RelationProxies.GetManualParentProxies().Select(x => x.Item.SourceId).ToArray();
                    }

                    if (!parentIds.Any())
                        return;

                    // Establish if this is Media or Content by checking the ancestors of the parents
                    // If not, don't do anything (e.g. it might be a member / user)
                    var allowedAncestors = new[] {FixedHiveIds.ContentVirtualRoot, FixedHiveIds.MediaVirtualRoot};
                    var isContentOrMedia = false;
                    foreach (var parentId in parentIds)
                    {
                        var ancestors = uow.Repositories.GetAncestorIds(parentId, FixedRelationTypes.DefaultRelationType).ToArray();
                        if (ancestors.Any(ancestor => allowedAncestors.Any(x => x.EqualsIgnoringProviderId(ancestor))))
                        {
                            isContentOrMedia = true;
                        }
                    }

                    if (!isContentOrMedia)
                    {
                        LogHelper.TraceIfEnabled<EnsureUniqueNameTask>("Not checking for unique name as item is not a descendent of Content or Media roots");
                        return;
                    }

                    var nodeNameAttr = item.Attributes[NodeNameAttributeDefinition.AliasValue];
                    var changed = false;
                    int duplicateNumber = 0;
                    var parentQuery = uow.Repositories.WithParentIds(parentIds); //.OfRevisionType(FixedStatusTypes.Published);
                    // Replace name
                    string name = string.Empty;
                    if (nodeNameAttr.Values.ContainsKey("Name"))
                    {
                        name = nodeNameAttr.Values["Name"].ToString();

                        // Check if there are any items matching the name
                        ChangeValue(nodeNameAttr, parentQuery, item, name, "Name", " (", " (1)", " ({0})");

                        changed = true;
                    }

                    // Replace url name
                    string urlName = string.Empty;
                    if (nodeNameAttr.Values.ContainsKey("UrlName"))
                    {
                        urlName = nodeNameAttr.Values["UrlName"].ToString();

                        // Check if there are any items matching the name
                        ChangeValue(nodeNameAttr, parentQuery, item, urlName, "UrlName", "-", "-1", "-{0}");

                        //var urlName = nodeNameAttr.Values["UrlName"].ToString();
                        //while (uow.Repositories.Query().InIds(siblings)
                        //    .OfRevisionType(FixedStatusTypes.Published)
                        //    .Any(x => x.InnerAttribute<string>(NodeNameAttributeDefinition.AliasValue, "UrlName") == urlName &&
                        //        x.Id != item.Id))
                        //{
                        //    var urlNameMatch = _urlNamePattern.Match(urlName);
                        //    if (urlNameMatch.Success)
                        //        urlName = urlName.TrimEnd(urlNameMatch.Groups[1].Value) + (Convert.ToInt32(urlNameMatch.Groups[1].Value) + 1);
                        //    else
                        //        urlName = urlName + "-1";
                        //}

                        //nodeNameAttr.Values["UrlName"] = urlName;
                        changed = true;
                    }

                    if (changed)
                    {
                        LogHelper.TraceIfEnabled<EnsureUniqueNameTask>("Changing name of item '{0}' (url: '{1}') as it wasn't unique", () => name, () => urlName);
                        uow.Repositories.Revisions.AddOrUpdate((Revision<TypedEntity>)args.Entity);
                        // Don't commit here; we're dealing with a unit of work that is owned by someone else
                    }
                }
            }
        }

        private static void ChangeValue(TypedAttribute nodeNameAttr, IQueryable<TypedEntity> parentQuery, TypedEntity item, string potentialValue, string valueKey, string existsSuffixCheck, string defaultSuffix, string dupeFormat)
        {
            var iterationCount = 0;
            var itExists = false;
            var originalValue = potentialValue;
            var valueCheck = potentialValue;
            do
            {
                iterationCount++;
                itExists = parentQuery.Any(x => x.InnerAttribute<string>(NodeNameAttributeDefinition.AliasValue, valueKey) == valueCheck && x.Id != item.Id);
                if (itExists)
                {
                    // Don't concatenate (1) (2) etc. just use the original value plus a count
                    valueCheck = originalValue + string.Format(dupeFormat, iterationCount);
                }
            } while (itExists);

            if (iterationCount > 1)
            {
                nodeNameAttr.Values[valueKey] = valueCheck;
            }

            //var exists = parentQuery
            //    .Any(x => x.InnerAttribute<string>(NodeNameAttributeDefinition.AliasValue, valueKey) == potentialValue && x.Id != item.Id);

            //if (exists)
            //{
            //    // Get the count of items matching potentialValue + "("
            //    var dupeName = potentialValue + existsSuffixCheck;
            //    var items = parentQuery
            //        .Where(
            //            x =>
            //            x.InnerAttribute<string>(NodeNameAttributeDefinition.AliasValue, valueKey).StartsWith(dupeName) &&
            //            x.Id != item.Id)
            //        .ToList();

            //    var itemsDebug = items.Select(x => new
            //        {
            //            x.Id,
            //            x.Attributes,
            //            ValueKey = x.InnerAttribute<string>(NodeNameAttributeDefinition.AliasValue, valueKey)
            //        }).ToArray();

            //    var count = items.Count;

            //    if (count == 0)
            //    {
            //        potentialValue = potentialValue + defaultSuffix;
            //    }
            //    else
            //    {
            //        var newCount = (count + 1);
            //        var format = string.Format(dupeFormat, newCount);
            //        potentialValue = potentialValue + format;
            //    }
            //}

            //nodeNameAttr.Values[valueKey] = potentialValue;
        }
    }
}
