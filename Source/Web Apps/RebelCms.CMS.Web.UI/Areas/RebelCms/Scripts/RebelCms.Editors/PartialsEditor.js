/// <reference path="/Areas/RebelCms/Scripts/RebelCms.System/NamespaceManager.js" />
/// <reference path="/Areas/RebelCms/Scripts/Base2/base2.js" />
/// <reference path="/Areas/RebelCms/Modules/RebelCmsTabs/RebelCmsTabs.js" />
/// <reference path="/Areas/RebelCms/Scripts/RebelCms.Editors/ScriptEditor.js" />

RebelCms.System.registerNamespace("RebelCms.Editors");

(function ($) {

    // Editor class
    RebelCms.Editors.PartialsEditor = RebelCms.Editors.ScriptEditor.extend({
            
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
                    title: "Insert an RebelCms page field",
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
                this._instance = new RebelCms.Editors.PartialsEditor();
            return this._instance;
        }
        
    });

})(jQuery);