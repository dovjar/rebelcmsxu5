/// <reference path="/Areas/Rebel/Scripts/Rebel.System/NamespaceManager.js" />
/// <reference path="/Areas/Rebel/Modules/RebelTabs/RebelTabs.js" />

Rebel.System.registerNamespace("Rebel.Editors");

(function ($, Base) {

    Rebel.Editors.MemberSearchEditor = Base.extend({
        
        _opts: null,
        _viewModel: null,
        
        constructor: function () {

            var _this = this;

            //view model for knockout js
            this._viewModel = $.extend({}, Rebel.System.BaseViewModel, {
                parent: this, // Allways set
                editorUrl: ko.observable(),
                searchTerm: ko.observable(""),
                searchResults: ko.observableArray([]),
                searching: ko.observable(false),
                noResults: ko.observable(false),
                search: function(e) {
                    
                    this.searching(true);
                    this.searchResults([]);
                    this.noResults(false);
                    
                    var data = ko.toJSON({ searchTerm: this.searchTerm() });

                    $.post(_this._opts.searchUrl, data, function(e) {
                        _this._viewModel.searching(false);
                        _this._viewModel.searchResults(e.members);
                        _this._viewModel.noResults(_this._viewModel.searchResults().length == 0);
                    });
                }
            });

        },

        init: function (o) {

            this._opts = $.extend({
                //the hidden field to track the active tab index
                activeTabIndexField: true,
                //the active tab index to show on load
                activeTabIndex: "",
                
                ajaxUrl: $("form").attr("action")
            }, o);

            this._viewModel.editorUrl(this._opts.editorUrl);

            //override the default index if it's zero and the query string exists
            if ($u.Sys.QueryStringHelper.getQueryStringValue("tabindex")) {
                this._opts.activeTabIndex = $u.Sys.QueryStringHelper.getQueryStringValue("tabindex");
            }
            
            //create the tabs			
            $("#tabs").rebelTabs({
                content: "#editorContent",
                activeTabIndex: this._opts.activeTabIndex,
                activeTabIndexField: this._opts.activeTabIndexField
            });

            ko.applyBindings(this._viewModel);
                
        }
        
    }, {
        
        _instance: null,
        
        // Singleton accessor
        getInstance: function () {
            if(this._instance == null)
                this._instance = new Rebel.Editors.MemberSearchEditor();
            return this._instance;
        }
        
    });

})(jQuery, base2.Base);