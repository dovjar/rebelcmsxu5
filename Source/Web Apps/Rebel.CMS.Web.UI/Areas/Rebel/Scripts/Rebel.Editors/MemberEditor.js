/// <reference path="/Areas/Rebel/Scripts/Rebel.System/NamespaceManager.js" />
/// <reference path="/Areas/Rebel/Modules/RebelTabs/RebelTabs.js" />

Rebel.System.registerNamespace("Rebel.Editors");

(function ($, Base) {

    Rebel.Editors.MemberEditor = Base.extend({
        
        init: function (o) {

            var _opts = $.extend({
                //the hidden field to track the active tab index
                activeTabIndexField: true,
                //the active tab index to show on load
                activeTabIndex: ""
            }, o);

            //override the default index if it's zero and the query string exists
            if ($u.Sys.QueryStringHelper.getQueryStringValue("tabindex")) {
                _opts.activeTabIndex = $u.Sys.QueryStringHelper.getQueryStringValue("tabindex");
            }
            
            //create the tabs			
            $("#tabs").rebelTabs({
                content: "#editorContent",
                activeTabIndex: _opts.activeTabIndex,
                activeTabIndexField: _opts.activeTabIndexField
            });

            //create the toolbar UI Element panel
            $("#editorBar .container").UIPanel("Default");
                
        }
        
    }, {
        
        _instance: null,
        
        // Singleton accessor
        getInstance: function () {
            if(this._instance == null)
                this._instance = new Rebel.Editors.MemberEditor();
            return this._instance;
        }
        
    });

})(jQuery, base2.Base);