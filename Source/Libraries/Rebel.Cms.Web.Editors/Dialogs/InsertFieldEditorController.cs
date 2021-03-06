﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Hive;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Editors.Dialogs
{
    [Editor(CorePluginConstants.InsertFieldEditorControllerId)]
    [RebelEditor]
    public class InsertFieldEditorController : AbstractEditorController
    {
        private ReadonlyGroupUnitFactory<IContentStore> _contentStore;

        public InsertFieldEditorController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        {
            _contentStore = BackOfficeRequestContext.Application.Hive.GetReader<IContentStore>();
        }

        [HttpGet]
        public ActionResult Index(HiveId id)
        {
            var model = new InsertFieldModel();

            using (var uow = _contentStore.CreateReadonly())
            {
                var schemaIds = uow.Repositories.Schemas.GetDescendentRelations(FixedHiveIds.ContentRootSchema, FixedRelationTypes.DefaultRelationType)
                    .DistinctBy(x => x.DestinationId)
                    .Select(x => x.DestinationId).ToArray();

                var schemas = uow.Repositories.Schemas.Get<EntitySchema>(true, schemaIds);
                var attributeDefs = schemas.SelectMany(x => x.AttributeDefinitions)
                    .DistinctBy(x => x.Alias);
                var attributeAliases = new List<string>();

                // This isn't ideal as we are currently hard coding which property editors
                // have complex data types. This will need to be updated when issue U5-332
                // is addressed.
                foreach (var attributeDef in attributeDefs)
                {
                    switch (attributeDef.AttributeType.RenderTypeProvider.ToUpper())
                    {
                        case CorePluginConstants.NodeNamePropertyEditorId:
                        case CorePluginConstants.SelectedTemplatePropertyEditorId:
                            break;
                        case CorePluginConstants.FileUploadPropertyEditorId:
                            attributeAliases.Add(attributeDef.Alias + " - MediaId");
                            attributeAliases.Add(attributeDef.Alias + " - Value");
                            break;
                        default:
                            attributeAliases.Add(attributeDef.Alias);
                            break;
                    }
                }

                attributeAliases.Add("@id");
                attributeAliases.Add("@parentId"); 
                attributeAliases.Add("@name");
                attributeAliases.Add("@urlName");
                attributeAliases.Add("@path");
                attributeAliases.Add("@level");
                attributeAliases.Add("@template");
                attributeAliases.Add("@templateId");
                attributeAliases.Add("@templateFileName");
                attributeAliases.Add("@nodeTypeAlias");
                attributeAliases.Add("@createDate");
                attributeAliases.Add("@updateDate");
                attributeAliases.Add("@statusChangedDate");

                model.AvailableFields = attributeAliases
                        .OrderBy(x => x.StartsWith("system-") || x.StartsWith("@") ? 1 : -1) // Force system fields to the bottom
                        .ThenBy(x => x);

                model.AvailableCasingTypes = Enum.GetNames(typeof(RebelRenderItemCaseType))
                    .OrderBy(x => x == "Unchanged" ? -1 : 1);

                model.AvailableEncodingTypes = Enum.GetNames(typeof(RebelRenderItemEncodingType))
                    .OrderBy(x => x == "Unchanged" ? -1 : 1);
            }

            return View(model);
        }
    }
}
