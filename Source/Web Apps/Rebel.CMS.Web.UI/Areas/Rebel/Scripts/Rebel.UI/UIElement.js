/// <reference path="/Areas/Rebel/Scripts/Base2/base2.js" />
/// <reference path="/Areas/Rebel/Scripts/Rebel.System/NamespaceManager.js" />
/// <reference path="/Areas/Rebel/Scripts/Rebel.System/UrlEncoder.js" />

Rebel.System.registerNamespace("Rebel.UI");

(function ($, Base) {

    // A class to represent a UI Element
    Rebel.UI.UIElement = Base.extend({
            
        _element: null,
            
        constructor: function (uiElementDef) {
            // constructor should be overriden in derived types to construct the element
            // base upon the passed in uiElementDef
        },
            
        bind: function () {
            // bind should be overridden in derived types to bind the UI elements events
        },
            
        getElement: function () {
            return this._element;
        }
        
    });

})(jQuery, base2.Base);