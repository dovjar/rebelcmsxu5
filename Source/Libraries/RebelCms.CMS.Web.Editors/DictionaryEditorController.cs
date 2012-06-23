using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Editors.Extenders;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Cms.Web.Mvc.ActionInvokers;
using RebelCms.Cms.Web.Mvc.ViewEngines;
using RebelCms.Framework;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Hive.ProviderGrouping;

namespace RebelCms.Cms.Web.Editors
{
    using RebelCms.Hive.RepositoryTypes;

    [Editor(CorePluginConstants.DictionaryEditorControllerId)]
    [RebelCmsEditor]
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
