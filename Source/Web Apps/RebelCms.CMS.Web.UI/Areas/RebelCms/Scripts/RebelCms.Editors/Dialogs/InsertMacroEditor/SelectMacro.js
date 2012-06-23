/// <reference path="/Areas/RebelCms/Scripts/RebelCms.System/NamespaceManager.js" />
/// <reference path="/Areas/RebelCms/Modules/RebelCmsTabs/RebelCmsTabs.js" />

RebelCms.System.registerNamespace("RebelCms.Editors.InsertMacroEditor");

(function ($, Base) {

    RebelCms.Editors.InsertMacroEditor.SelectMacro = Base.extend({

        _editorViewModel: null,

        constructor: function () {

            this._editorViewModel = $.extend({}, RebelCms.System.BaseViewModel, {
                parent: this, // Allways set
                macroAlias: ko.observable(),
                cancel: function (e) {
                    e.preventDefault();

                    // Close the dialog
                    window.parent.$u.Sys.WindowManager.getInstance().hideModal();
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
                this._instance = new RebelCms.Editors.InsertMacroEditor.SelectMacro();
            return this._instance;
        }

    });

})(jQuery, base2.Base);