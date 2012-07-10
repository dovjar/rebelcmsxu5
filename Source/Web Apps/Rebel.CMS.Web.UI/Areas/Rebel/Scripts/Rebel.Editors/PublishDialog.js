/// <reference path="/Areas/Rebel/Scripts/Rebel.System/NamespaceManager.js" />

Rebel.System.registerNamespace("Rebel.Editors");

(function ($, Base) {

    Rebel.Editors.PublishDialog = Base.extend({
        
        _opts: null,

        _dialogViewModel: null,
            
        constructor: function () {

            var _this = this;
            
            this._dialogViewModel = $.extend({}, Rebel.System.BaseViewModel, {
                parent: this, // Allways set
                includeChildren: ko.observable(false),
                includeUnpublishedChildren: ko.observable(false),
                msgText: ko.observable(""),
                id: null,
                success: ko.observable(false),
                save: function(val, e) {
                    e.preventDefault();
                    $("#submit_Save").hide().parent().append("<div class='progress-spinner'/>");
                    var _this = this;
                    var data = ko.toJSON(_this);
                    $.post(_this.parent._opts.ajaxUrl, data, function(e) {
                        $("#submit_Save").show().parent().find(".progress-spinner").remove();
                        //check result for errors and use our custom validator
                        if (Rebel.System.ValidationHelper.validateJsonResponse(e, "CustomDataValidationRule")) {
                            $(".validation-summary").validationSummaryApi().hideSummary();
                            _this.success(e.success);
                            _this.msgText(e.msg);
                            Rebel.System.NotificationManager.getInstance().showNotifications(e.notifications);
                            
                            //now sync
                            $u.Sys.ApiMgr.getMainTree().syncNode(e.path[0]);
                        }
                    });
                }
            });
            
            //uncheck the includeUnpublishedChildren property if the includeChildren property is unchecked
            this._dialogViewModel.includeChildren.subscribe(function(newValue) {            
                if (!newValue) _this._dialogViewModel.includeUnpublishedChildren(false);
            });
            
        },

        nodeClickHandler: function(e, args) {
            ///<summary>Handles the tree node click</summary>
                
            var id = args.node.attr("id");

            //set the view model properties
            this._dialogViewModel.toId(id);
            this._dialogViewModel.hasSelectedNode(true);
            this._dialogViewModel.msgText("'" + args.node.find("span:first").text() + "' has been selected as the root of your new content, click 'Save'.");
        },

        init: function(o) {            
                        
            this._opts = o;

            //init the view model properties
            this._dialogViewModel.id = this._opts.id;

            //manually add ko-bindings to the form elements
            $("#submit_Save").attr("data-bind", "click: save, visible: !success()");                
                
            //enabled the standard validation engine
            $.validator.unobtrusive.reParse($("form"));

            //apply knockout js bindings
            ko.applyBindings(this._dialogViewModel);
        }

    }, {
        
        _instance: null,
        
        // Singleton accessor
        getInstance: function () {
            if(this._instance == null)
                this._instance = new Rebel.Editors.PublishDialog();
            return this._instance;
        }
        
    });

})(jQuery, base2.Base);