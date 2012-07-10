/// <reference path="/Areas/Rebel/Scripts/Rebel.System/NamespaceManager.js" />

Rebel.System.registerNamespace("Rebel.Controls.TinyMCE");

(function ($) {

    Rebel.Controls.TinyMCE.SelectMacro = function() {
        
        var viewModel = {
            macroAlias: ko.observable("")
        };

        return {            
            init: function() {

                //apply knockout js bindings
                ko.applyBindings(viewModel);
            }
        }

    }(); //singleton    

})(jQuery);