/// <reference path="../../Scripts/RebelCms.System/NamespaceManager.js" />

RebelCms.System.registerNamespace("RebelCms.Editors");

(function ($, Base) {

    RebelCms.Editors.PackageInstaller = Base.extend({

        _viewModel: null,
            
        constructor: function () {

            this._viewModel = $.extend({}, RebelCms.System.BaseViewModel, {
                parent: this, // Allways set
                autoInstall: ko.observable(false),
                acceptTerms: ko.observable(false),
                validateInstall: function() {
                    return confirm("Are you sure you want to install this package?");
                },
                validateRemoval: function() {
                    return confirm("This will delete this package from your local repository, are you sure you want to remove it?");
                },
                validateUninstall: function() {
                    return confirm("Are you sure you want to uninstall this package?");
                }
            });
            
            this._viewModel.canUpload = ko.dependentObservable(function() {
                return !this.autoInstall() || (this.autoInstall() && this.acceptTerms());
            }, this._viewModel);
        },

        init: function() {
            
            // Force app tray to reload 
            //TODO: Need a way to do this only when we want to, rather than on every request
            $u.Sys.ClientApiManager.getApp().refreshAppTray();

            //apply knockout js bindings
            ko.applyBindings(this._viewModel);
        }
            
    }, {
        
        _instance: null,
        
        // Singleton accessor
        getInstance: function () {
            if(this._instance == null)
                this._instance = new RebelCms.Editors.PackageInstaller();
            return this._instance;
        }
        
    });
    
})(jQuery, base2.Base);