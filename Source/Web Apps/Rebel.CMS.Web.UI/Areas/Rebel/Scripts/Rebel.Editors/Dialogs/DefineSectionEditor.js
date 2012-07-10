/// <reference path="/Areas/Rebel/Scripts/Rebel.System/NamespaceManager.js" />
/// <reference path="/Areas/Rebel/Modules/RebelTabs/RebelTabs.js" />

Rebel.System.registerNamespace("Rebel.Editors");

(function ($, Base) {

    Rebel.Editors.DefineSectionEditor = Base.extend({

        _editorViewModel: null,

        constructor: function () {

            this._editorViewModel = $.extend({}, Rebel.System.BaseViewModel, {
                parent: this, // Allways set
                type: ko.observable("body"),
                name: ko.observable(),
                required: ko.observable(false),
                save: function(val, e) {
                    e.preventDefault();
                    
                    // Build up code snippet
                    var snippet = (this.type() == "body") ? "@RenderBody()" : "@RenderSection(\""+ this.name() +"\", "+ this.required().toString().toLowerCase() +")";
                    
                    // Insert snippet into current editors code mirror instance
                    window.parent.editor.getCodeMirrorInstance().replaceSelection(snippet);
                    window.parent.editor.getCodeMirrorInstance().focus();
                    
                    // Close the dialog
                    this.cancel(e);
                },
                cancel: function(e) {
                    e.preventDefault();
                    
                    // Close the dialog
                    window.parent.$u.Sys.WindowManager.getInstance().hideModal({
                        id: "definesection"
                    });
                }
            });

        },

        init: function () {

            //apply knockout js bindings
            ko.applyBindings(this._editorViewModel);
            
        }
    }, {

        _instance: null,

        // Singleton accessor
        getInstance: function () {
            if (this._instance == null)
                this._instance = new Rebel.Editors.DefineSectionEditor();
            return this._instance;
        }

    });

})(jQuery, base2.Base);