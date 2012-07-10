/// <reference path="/Areas/Rebel/Scripts/Rebel.System/NamespaceManager.js" />
/// <reference path="/Areas/Rebel/Modules/RebelTabs/RebelTabs.js" />

Rebel.System.registerNamespace("Rebel.Editors");

(function ($, Base) {

    Rebel.Editors.DataTypeEditor = Base.extend({
            
        init: function (o) {

            var _opts = $.extend({
                //jquery selector for the proeprty editor drop down list
                propEditorDropDown: false
            }, o);

            //create the tabs			
            $("#tabs").rebelTabs({
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
                this._instance = new Rebel.Editors.DataTypeEditor();
            return this._instance;
        }

    });     

})(jQuery, base2.Base);