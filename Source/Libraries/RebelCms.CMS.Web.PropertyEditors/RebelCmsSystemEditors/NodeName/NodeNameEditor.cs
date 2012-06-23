using System;
using System.Collections;
using RebelCms.Cms.Web.Configuration.RebelCmsSystem;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;
using RebelCms.Framework;

namespace RebelCms.Cms.Web.PropertyEditors.RebelCmsSystemEditors.NodeName
{

    [RebelCmsPropertyEditor]
    [PropertyEditor(CorePluginConstants.NodeNamePropertyEditorId, "NodeNameEditor", "Node Name Editor")]
    public class NodeNameEditor : PropertyEditor<NodeNameEditorModel>
    {
        private IRebelCmsApplicationContext _appContext;

        public NodeNameEditor(IRebelCmsApplicationContext appContext)
        {
            _appContext = appContext;
        }

        public override NodeNameEditorModel CreateEditorModel()
        {
            Func<string, string> urlifyDelegate = (name) =>
            {
                var replacementDict = new global::System.Collections.Generic.Dictionary<string, string>();
                _appContext.Settings.Urls.CharReplacements.ForEach(x => replacementDict.Add(x.Char, x.Value));

                return name.ToUrlAlias(replacementDict, 
                    _appContext.Settings.Urls.RemoveDoubleDashes, 
                    _appContext.Settings.Urls.StripNonAscii,
                    _appContext.Settings.Urls.UrlEncode);
            };

            return new NodeNameEditorModel(urlifyDelegate);
        }

    }
}
