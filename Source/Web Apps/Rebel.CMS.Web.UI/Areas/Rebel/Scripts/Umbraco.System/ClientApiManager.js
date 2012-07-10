/// <reference path="/Areas/Rebel/Scripts/Rebel.System/NamespaceManager.js" />

Rebel.System.registerNamespace("Rebel.System");

(function ($, Base) {

    Rebel.System.ClientApiManager = Base.extend(null, {
        ///<summary>A helper static class that provides access to the main existing objects that govern the application</summary>

        getApp: function() {
            return window.top.jQuery(window.top).rebelApplicationApi();
        },

        getMainTree: function() {
            ///<summary>returns the api for the main left column tree</summary>
            return window.top.jQuery("#mainTree").rebelTreeApi();
        },

        getHistoryManager: function() {
            ///<summary>returns the history manager for the top most window = the only one there should be</summary>
            return Rebel.System.HistoryManager.getManagerForWindow(window.top);
        }
    });

    //create the shorthand notation
    
    //first the namespaces
    $u = Rebel;
    $u.Sys = Rebel.System;
    $u.Ctl = Rebel.Controls;

    //then common objects
    $u.Sys.ApiMgr = $u.Sys.ClientApiManager;

})(jQuery, base2.Base);