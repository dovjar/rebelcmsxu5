/// <reference path="/Areas/RebelCms/Scripts/RebelCms.System/NamespaceManager.js" />

RebelCms.System.registerNamespace("RebelCms.Controls.TinyMCE");

(function ($) {

    RebelCms.Controls.TinyMCE.SelectMacro = function() {
        
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