/// <reference path="/Areas/Rebel/Scripts/Base2/base2.js" />
/// <reference path="/Areas/Rebel/Scripts/Rebel.System/NamespaceManager.js" />
/// <reference path="/Areas/Rebel/Scripts/Rebel.System/BaseViewModel.js" />

(function ($) {

    $(function () {

        //adds a copy to clipboard link next to all hive ids
        $("span.hive-id").append("<span class='copy-hive-id-wrapper'><a href='javascript:void(0);' class='copy-hive-id' title='Copy to clipboard'><img src='../../../../areas/rebel/content/images/clipboard.png' alt='Copy to clipboard' /></a></span>");

        $("span.hive-id a.copy-hive-id").zclip({
            path: '../../../../areas/rebel/modules/zclip/ZeroClipboard.swf',
            copy: function () { return $('span.hive-id').text(); },
            afterCopy: function () {
                $u.Sys.NotificationManager.getInstance().showNotifications([{ title: "Id Copied Successfully", message: "The Id has been copied to the clipboard successfully", type: "success"}]);
            }
        });

    });

})(jQuery);