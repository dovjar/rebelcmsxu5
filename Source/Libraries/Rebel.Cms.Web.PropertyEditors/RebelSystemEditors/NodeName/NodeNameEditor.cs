using System;
using System.Collections;
using Rebel.Cms.Web.Configuration.RebelSystem;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;
using Rebel.Framework;

namespace Rebel.Cms.Web.PropertyEditors.RebelSystemEditors.NodeName
{

    [RebelPropertyEditor]
    [PropertyEditor(CorePluginConstants.NodeNamePropertyEditorId, "NodeNameEditor", "Node Name Editor")]
    public class NodeNameEditor : PropertyEditor<NodeNameEditorModel>
    {
        private IRebelApplicationContext _appContext;

        public NodeNameEditor(IRebelApplicationContext appContext)
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
