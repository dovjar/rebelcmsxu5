/// <reference path="/Areas/Rebel/Scripts/Rebel.System/NamespaceManager.js" />
/// <reference path="/Areas/Rebel/Modules/RebelTabs/RebelTabs.js" />

Rebel.System.registerNamespace("Rebel.Editors.InsertMacroEditor");

(function ($, Base) {

    Rebel.Editors.InsertMacroEditor.SelectMacro = Base.extend({

        _editorViewModel: null,

        constructor: function () {

            this._editorViewModel = $.extend({}, Rebel.System.BaseViewModel, {
                parent: this, // Allways set
                macroAlias: ko.observable(),
                cancel: function (e) {
                    e.preventDefault();

                    // Close the dialog
                    window.parent.$u.Sys.WindowManager.getInstance().hideModal({
                        id: "insertmacro"
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
                this._instance = new Rebel.Editors.InsertMacroEditor.SelectMacro();
            return this._instance;
        }

    });

})(jQuery, base2.Base);