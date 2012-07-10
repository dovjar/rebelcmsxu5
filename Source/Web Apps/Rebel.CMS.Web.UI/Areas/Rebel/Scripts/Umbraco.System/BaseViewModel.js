/// <reference path="/Areas/Rebel/Scripts/Rebel.System/NamespaceManager.js" />
/// <reference path="/Areas/Rebel/Scripts/Base2/base2.js" />
/// <reference path="/Areas/Rebel/Scripts/KnockoutJs/knockout-2.0.js" />

Rebel.System.registerNamespace("Rebel.System");

(function ($) {

    // A base class for knockout js view models
    // NB: At the time of writing, using Knockout observables and Base2 was causing issues
    // when used on the same entity together, so for view models we use plain old objects
    // and use the jquery $.extend method to do a kind of inheritance. With this, you can
    // inherit properties / methods, but you can't override them, only replace them.
    Rebel.System.BaseViewModel = {

        parent: null,
            
        toJSON: function () {
            var copy = ko.toJS(this);
            delete copy.parent; 
            return copy; 
        }

    };

})(jQuery);