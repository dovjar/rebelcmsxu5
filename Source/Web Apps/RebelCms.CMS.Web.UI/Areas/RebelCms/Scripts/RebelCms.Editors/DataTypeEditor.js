/// <reference path="/Areas/RebelCms/Scripts/RebelCms.System/NamespaceManager.js" />
/// <reference path="/Areas/RebelCms/Modules/RebelCmsTabs/RebelCmsTabs.js" />

RebelCms.System.registerNamespace("RebelCms.Editors");

(function ($, Base) {

    RebelCms.Editors.DataTypeEditor = Base.extend({
            
        init: function (o) {

            var _opts = $.extend({
                //jquery selector for the proeprty editor drop down list
                propEditorDropDown: false
            }, o);

            //create the tabs			
            $("#tabs").RebelCmsTabs({
                content: "#editorContent"
            });
        
            //The knockout js view model for the selected item
            var selectedPropEditorViewModel = {
                selectedId: ko.observable(_opts.propEditorDropDown.val()),
                itemChanged: function (event) {
                    this.selectedId($(event.target).val());
                }
            };

            ko.applyBindings(selectedPropEditorViewModel, document.getElementById('dataTypeDefinition'));
        }

    }, {

        _instance: null,

        // Singleton accessor
        getInstance: function () {
            if (this._instance == null)
                this._instance = new RebelCms.Editors.DataTypeEditor();
            return this._instance;
        }

    });     

})(jQuery, base2.Base);