/// <reference path="/Areas/Rebel/Scripts/Base2/base2.js" />
/// <reference path="/Areas/Rebel/Scripts/Rebel.System/NamespaceManager.js" />
/// <reference path="/Areas/Rebel/Scripts/Rebel.System/UrlEncoder.js" />

Rebel.System.registerNamespace("Rebel.UI.UIElements");

(function ($, Base) {

    // A class to represent a Seperator UI Element
    Rebel.UI.UIElements.SeperatorUIElement = Rebel.UI.UIElement.extend({

        constructor: function (uiElementDef) {
            // Create element
            this._element = $("<span class='seperator-ui-element'></span>");
        }

    });

})(jQuery, base2.Base);