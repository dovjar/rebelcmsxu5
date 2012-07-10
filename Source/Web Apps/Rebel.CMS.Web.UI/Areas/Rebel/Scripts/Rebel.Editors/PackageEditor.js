/// <reference path="/Areas/Rebel/Scripts/Rebel.System/NamespaceManager.js" />
/// <reference path="/Areas/Rebel/Modules/RebelTabs/RebelTabs.js" />

Rebel.System.registerNamespace("Rebel.Editors");

(function ($, Base) {

    Rebel.Editors.PackageEditor = Base.extend({
        
        _isCreated: false,
        _packageViewModel: null,
        
        constructor: function () {
            
            this._packageViewModel = $.extend({}, Rebel.System.BaseViewModel, {
                parent: this, // Allways set
                additionalFilePath: ko.observable(""),
                additionalFiles: ko.observableArray([]),
                addFile: function () {
                    this.parent._addFile(this.additionalFilePath());
                    this.additionalFilePath("");
                    return false;
                },
                browseFile: function () {
                    
                    var _this = this;
                    
                    // Show tree modal window
                    $u.Sys.WindowManager.getInstance().showModal({
                        id: "treepicker_browse",
                        title: "Browse",
                        isGlobal: false,
                        forceContentInIFrame: false,
                        content: "#browse-tree",
                        modalClass: "tree-picker",
                        removeOnHide: false
                    });

                    // Initialize the tree
                    if (!_this.parent._isCreated) {
                        $("#browse-tree").rebelTreeApi().createJsTree();
                        _this.parent._isCreated = true;
                    }

                    return false;
                }
            });

        },
        
        _addFile: function (path) {
            var _this = this;
            this._packageViewModel.additionalFiles.unshift({ 
                path: path,
                removeFile: function () {
                    _this._packageViewModel.additionalFiles.remove(this);
                    return false;
                }
            });
        },

        init: function (o) {

            var _opts = $.extend({
                //the hidden field to track the active tab index
                activeTabIndexField: true,
                //the active tab index to show on load
                activeTabIndex: "",
                additionalFiles: []
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

            var i = _opts.additionalFiles.length;
            while (i--) {
                this._addFile(_opts.additionalFiles[i]);
            }

            //apply knockout js bindings
            ko.applyBindings(this._packageViewModel, $(".additional-file-list").get(0));
        }
        
    }, {
        
        _instance: null,
        
        // Singleton accessor
        getInstance: function () {
            if(this._instance == null)
                this._instance = new Rebel.Editors.PackageEditor();
            return this._instance;
        },
        
        nodeClickHandler: function (e, data) {

            // Close the tree picker
            $u.Sys.WindowManager.getInstance().hideModal({
                id: "treepicker_browse",
                isGlobal: false
            });

            // Save value
            Rebel.Editors.PackageEditor.getInstance()._packageViewModel.additionalFilePath(data.metaData.jsonId.value);
        }
        
    });

})(jQuery, base2.Base);