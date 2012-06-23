/// <reference path="/Areas/RebelCms/Scripts/Base2/base2.js" />
/// <reference path="/Areas/RebelCms/Scripts/RebelCms.System/NamespaceManager.js" />
/// <reference path="/Areas/RebelCms/Scripts/RebelCms.System/UrlEncoder.js" />

RebelCms.System.registerNamespace("RebelCms.UI.UIElements");

(function ($, Base) {

    // A class to represent a Seperator UI Element
    RebelCms.UI.UIElements.SeperatorUIElement = RebelCms.UI.UIElement.extend({

        constructor: function (uiElementDef) {
            // Create element
            this._element = $("<span class='seperator-ui-element'></span>");
        }

    });

})(jQuery, base2.Base);