/// <reference path="/Areas/RebelCms/Scripts/RebelCms.System/NamespaceManager.js" />

RebelCms.System.registerNamespace("RebelCms.System");

(function ($, Base) {

    RebelCms.System.QueryStringHelper = Base.extend(null, {
        getQueryStringValue: function(qryName) {
            var query = location.search.substring(1);
            var parts = query.split("&");
            for (var i = 0; i < parts.length; i++) {
                var token = parts[i].split("=");
                if (token[0] == qryName) {
                    return token[1];
                }
            }
            return null;
        }
    });

})(jQuery, base2.Base);