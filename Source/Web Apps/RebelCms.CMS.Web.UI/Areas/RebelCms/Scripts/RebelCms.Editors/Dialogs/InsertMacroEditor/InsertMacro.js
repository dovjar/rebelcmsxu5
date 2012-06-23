/// <reference path="/Areas/RebelCms/Scripts/RebelCms.System/NamespaceManager.js" />
/// <reference path="/Areas/RebelCms/Modules/RebelCmsTabs/RebelCmsTabs.js" />

RebelCms.System.registerNamespace("RebelCms.Editors.InsertMacroEditor");

(function ($, Base) {

    RebelCms.Editors.InsertMacroEditor.InsertMacro = Base.extend({

        init: function (o) {

            // Build up code snippet
            var snippet = "@RebelCms.RenderMacro(\"" + o.macroAlias +"\"";
            
            var params = "";
            for(var paramName in o.macroParams) {
                if(params != "")
                    params += ", ";
                params += paramName + " = \"" + o.macroParams[paramName] +"\"";
            }
            
            if(params != "") {
                snippet += ", new { " + params + " }";
            }
            
            snippet += ")";
            
            // Insert snippet into current editors code mirror instance
            window.parent.editor.getCodeMirrorInstance().replaceSelection(snippet);
            window.parent.editor.getCodeMirrorInstance().focus();
            
            // Close the dialog
            window.parent.$u.Sys.WindowManager.getInstance().hideModal();

        }
        
    }, {

        _instance: null,

        // Singleton accessor
        getInstance: function () {
            if (this._instance == null)
                this._instance = new RebelCms.Editors.InsertMacroEditor.InsertMacro();
            return this._instance;
        }

    });

})(jQuery, base2.Base);