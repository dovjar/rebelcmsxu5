/// <reference path="/Areas/RebelCms/Scripts/RebelCms.System/NamespaceManager.js" />
/// <reference path="/Areas/RebelCms/Modules/RebelCmsTabs/RebelCmsTabs.js" />

RebelCms.System.registerNamespace("RebelCms.Editors");

(function ($, Base) {

    RebelCms.Editors.DefaultDashboard = Base.extend({
            
        init: function(o) {

            //create the tabs			
            $("#tabs").RebelCmsTabs({
                content: "#dashboardContent",
                activeTabIndex: o.activeTabIndex,
                activeTabIndexField: o.activeTabIndexField
            });
            
        }
        
    }, {

        _instance: null,

        // Singleton accessor
        getInstance: function () {
            if (this._instance == null)
                this._instance = new RebelCms.Editors.DefaultDashboard();
            return this._instance;
        }

    });

})(jQuery, base2.Base);