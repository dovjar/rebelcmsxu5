/// <reference path="../../Scripts/Rebel.System/NamespaceManager.js" />
/// <reference path="../../Scripts/Rebel.Rebel.System/UrlEncoder.js" />

Rebel.System.registerNamespace("Rebel.PropertyEditors");

(function ($, Base) {

    // Initializes a new instance of the PropertyEditor class.
    // Should be extended by property editors with any additional functionality needed.
    Rebel.PropertyEditors.PropertyEditor = Base.extend({
            
        id : null,
        context : null,
        
        constructor: function(id){
            this.id = id;
        }
        
    });
     
})(jQuery, base2.Base);