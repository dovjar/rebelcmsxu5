/// <reference path="/Areas/Rebel/Scripts/Rebel.System/NamespaceManager.js" />
/// <reference path="/Areas/Rebel/Modules/RebelTabs/RebelTabs.js" />

Rebel.System.registerNamespace("Rebel.Editors");

(function ($, Base) {

    Rebel.Editors.DefaultDashboard = Base.extend({
            
        init: function(o) {

            //create the tabs			
            $("#tabs").rebelTabs({
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
                this._instance = new Rebel.Editors.DefaultDashboard();
            return this._instance;
        }

    });

})(jQuery, base2.Base);