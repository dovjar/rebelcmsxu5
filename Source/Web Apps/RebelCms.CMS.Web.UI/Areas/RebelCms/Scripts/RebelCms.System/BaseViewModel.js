/// <reference path="/Areas/RebelCms/Scripts/RebelCms.System/NamespaceManager.js" />
/// <reference path="/Areas/RebelCms/Scripts/Base2/base2.js" />
/// <reference path="/Areas/RebelCms/Scripts/KnockoutJs/knockout-1.2.1.js" />

RebelCms.System.registerNamespace("RebelCms.System");

(function ($) {

    // A base class for knockout js view models
    // NB: At the time of writing, using Knockout observables and Base2 was causing issues
    // when used on the same entity together, so for view models we use plain old objects
    // and use the jquery $.extend method to do a kind of inheritance. With this, you can
    // inherit properties / methods, but you can't override them, only replace them.
    RebelCms.System.BaseViewModel = {

        parent: null,
            
        toJSON: function () {
            var copy = ko.toJS(this);
            delete copy.parent; 
            return copy; 
        }

    };

})(jQuery);