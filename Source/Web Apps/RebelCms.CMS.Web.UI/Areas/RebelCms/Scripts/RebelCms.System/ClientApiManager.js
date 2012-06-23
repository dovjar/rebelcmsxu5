/// <reference path="/Areas/RebelCms/Scripts/RebelCms.System/NamespaceManager.js" />

RebelCms.System.registerNamespace("RebelCms.System");

(function ($, Base) {

    RebelCms.System.ClientApiManager = Base.extend(null, {
        ///<summary>A helper static class that provides access to the main existing objects that govern the application</summary>

        getApp: function() {
            return window.top.jQuery(window.top).RebelCmsApplicationApi();
        },

        getMainTree: function() {
            ///<summary>returns the api for the main left column tree</summary>
            return window.top.jQuery("#mainTree").RebelCmsTreeApi();
        },

        getHistoryManager: function() {
            ///<summary>returns the history manager for the top most window = the only one there should be</summary>
            return RebelCms.System.HistoryManager.getManagerForWindow(window.top);
        }
    });

    //create the shorthand notation
    
    //first the namespaces
    $u = RebelCms;
    $u.Sys = RebelCms.System;
    $u.Ctl = RebelCms.Controls;

    //then common objects
    $u.Sys.ApiMgr = $u.Sys.ClientApiManager;

})(jQuery, base2.Base);