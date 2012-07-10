using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Editors.Extenders;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Mvc.ActionInvokers;
using Rebel.Cms.Web.Mvc.ViewEngines;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Hive.ProviderGrouping;

namespace Rebel.Cms.Web.Editors
{
    using Rebel.Hive.RepositoryTypes;

    [Editor(CorePluginConstants.DictionaryEditorControllerId)]
    [RebelEditor]
    [AlternateViewEnginePath("ContentEditor")]
    [ExtendedBy(typeof(MoveCopyController), AdditionalParameters = new object[] { CorePluginConstants.DictionaryTreeControllerId })]
    public class DictionaryEditorController : AbstractRevisionalContentEditorController<DictionaryItemEditorModel>
    {
        private readonly GroupUnitFactory _hive;
        private readonly ReadonlyGroupUnitFactory _readonlyHive;

        public DictionaryEditorController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        {
            _hive = BackOfficeRequestContext.Application.Hive.GetWriter(new Uri("dictionary://"));
            _readonlyHive = BackOfficeRequestContext.Application.Hive.GetReader<IContentStore>();

            Mandate.That(_hive != null, x => new NullReferenceException("Could not find hive provider for route dictionary://"));
        }

        public override GroupUnitFactory Hive
        {
            get { return _hive; }
        }

        public override ReadonlyGroupUnitFactory ReadonlyHive
        {
            get
            {
                return _readonlyHive;
            }
        }

        public override HiveId VirtualRootNodeId
        {
            get { return FixedHiveIds.DictionaryVirtualRoot; }
        }

        public override HiveId RootSchemaNodeId
        {
            get { return FixedHiveIds.DictionaryRootSchema; }
        }

        public override HiveId RecycleBinId
        {
            get { return HiveId.Empty; }
        }

        public override string CreateNewTitle
        {
            get { return "Create new Dictionary Item"; }
        }
    }
}
