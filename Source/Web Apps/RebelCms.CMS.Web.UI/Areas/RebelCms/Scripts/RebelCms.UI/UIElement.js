/// <reference path="/Areas/RebelCms/Scripts/Base2/base2.js" />
/// <reference path="/Areas/RebelCms/Scripts/RebelCms.System/NamespaceManager.js" />
/// <reference path="/Areas/RebelCms/Scripts/RebelCms.System/UrlEncoder.js" />

RebelCms.System.registerNamespace("RebelCms.UI");

(function ($, Base) {

    // A class to represent a UI Element
    RebelCms.UI.UIElement = Base.extend({
            
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