/// <reference path="/Areas/Rebel/Scripts/Rebel.System/NamespaceManager.js" />
/// <reference path="/Areas/Rebel/Scripts/Base2/base2.js" />
/// <reference path="/Areas/Rebel/Modules/RebelTabs/RebelTabs.js" />
/// <reference path="/Areas/Rebel/Scripts/Rebel.Editors/ScriptEditor.js" />

Rebel.System.registerNamespace("Rebel.Editors");

(function ($) {

    // Editor class
    Rebel.Editors.PartialsEditor = Rebel.Editors.ScriptEditor.extend({
            
        // Public
        init: function(o){
            
            // Call base constructor
            this.base(o);

            _this = this;
            
            // Toolbar buttons
            $("#submit_InsertField").click(function(e) {
                e.preventDefault();
                
                // Show tree modal window
                $u.Sys.WindowManager.getInstance().showModal({
                    id: "insertfield",
                    title: "Insert an rebel page field",
                    isGlobal: false,
                    forceContentInIFrame: true,
                    contentUrl: o.insertFieldUrl,
                    modalClass: "insert-field",
                    removeOnHide: false
                });
                
            });
            
            $("#submit_InsertMacro").click(function(e) {
                e.preventDefault();
                
                // Show tree modal window
                $u.Sys.WindowManager.getInstance().showModal({
                    id: "insertmacro",
                    title: "Insert a Macro",
                    isGlobal: false,
                    forceContentInIFrame: true,
                    contentUrl: o.insertMacroUrl,
                    modalClass: "insert-field",
                    removeOnHide: false
                });
                
            });
        }
            
    }, {
        
        _instance: null,
        
        // Singleton accessor
        getInstance: function () {
            if(this._instance == null)
                this._instance = new Rebel.Editors.PartialsEditor();
            return this._instance;
        }
        
    });

})(jQuery);