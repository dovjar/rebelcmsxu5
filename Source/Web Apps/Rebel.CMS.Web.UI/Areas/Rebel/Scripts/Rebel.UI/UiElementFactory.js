/// <reference path="/Areas/Rebel/Scripts/Base2/base2.js" />
/// <reference path="../../Scripts/Rebel.System/NamespaceManager.js" />
/// <reference path="../../Scripts/Rebel.System/UrlEncoder.js" />

Rebel.System.registerNamespace("Rebel.UI");

(function ($, Base) {

    // Singleton UI Element Factory class to encapsulate the creation of UI Elements.
    Rebel.UI.UIElementFactory = Base.extend({

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
                this._instance = new Rebel.UI.UIElementFactory();
            return this._instance;
        }
        
    });

})(jQuery, base2.Base);