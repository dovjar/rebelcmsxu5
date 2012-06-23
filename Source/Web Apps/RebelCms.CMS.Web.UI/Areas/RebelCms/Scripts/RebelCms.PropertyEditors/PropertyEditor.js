/// <reference path="../../Scripts/RebelCms.System/NamespaceManager.js" />
/// <reference path="../../Scripts/RebelCms.RebelCms.System/UrlEncoder.js" />

RebelCms.System.registerNamespace("RebelCms.PropertyEditors");

(function ($, Base) {

    // Initializes a new instance of the PropertyEditor class.
    // Should be extended by property editors with any additional functionality needed.
    RebelCms.PropertyEditors.PropertyEditor = Base.extend({
            
        id : null,
        context : null,
        
        constructor: function(id){
            this.id = id;
        }
        
    });
     
})(jQuery, base2.Base);