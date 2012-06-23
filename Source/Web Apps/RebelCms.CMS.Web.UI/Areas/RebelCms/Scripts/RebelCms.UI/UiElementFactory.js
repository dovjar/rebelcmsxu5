/// <reference path="/Areas/RebelCms/Scripts/Base2/base2.js" />
/// <reference path="../../Scripts/RebelCms.System/NamespaceManager.js" />
/// <reference path="../../Scripts/RebelCms.System/UrlEncoder.js" />

RebelCms.System.registerNamespace("RebelCms.UI");

(function ($, Base) {

    // Singleton UI Element Factory class to encapsulate the creation of UI Elements.
    RebelCms.UI.UIElementFactory = Base.extend({

        // Create a UI dom element from a UI Element entity
        createUIElement : function (uiElementDef) {
            var uiElementType = uiElementDef.jsType.toFunction();
            return new uiElementType(uiElementDef);
        }

    }, {
        
        _instance: null,
        
        // Singleton accessor
        getInstance: function () {
            if(this._instance == null)
                this._instance = new RebelCms.UI.UIElementFactory();
            return this._instance;
        }
        
    });

})(jQuery, base2.Base);