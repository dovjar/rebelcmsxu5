/// <reference path="/Areas/Rebel/Scripts/Rebel.System/NamespaceManager.js" />
/// <reference path="/Areas/Rebel/Modules/RebelTabs/RebelTabs.js" />

Rebel.System.registerNamespace("Rebel.Editors.InsertMacroEditor");

(function ($, Base) {

    Rebel.Editors.InsertMacroEditor.InsertMacro = Base.extend({

        init: function (o) {

            // Build up code snippet
            var snippet = "@Rebel.RenderMacro(\"" + o.macroAlias +"\"";
            
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
            window.parent.$u.Sys.WindowManager.getInstance().hideModal({
                id: "insertmacro"
            });

        }
        
    }, {

        _instance: null,

        // Singleton accessor
        getInstance: function () {
            if (this._instance == null)
                this._instance = new Rebel.Editors.InsertMacroEditor.InsertMacro();
            return this._instance;
        }

    });

})(jQuery, base2.Base);