(function () {
    tinymce.create('tinymce.plugins.RebelCmsSave', {
        init: function (ed, url) {
            var t = this;
            t.editor = ed;
            ed.addCommand('mceSave', t._save, t);
            ed.addShortcut('ctrl+s', ed.getLang('save.save_desc'), 'mceSave');
        },

        getInfo: function () {
            return {
                longname: 'RebelCms Save',
                author: 'RebelCms HQ',
                authorurl: 'http://RebelCms.com',
                infourl: 'http://RebelCms.com',
                version: "1.0"
            };
        },

        // Private methods
        _save: function () {
            // Just pass to the RebelCms save button
            $("*[data-shortcut*='ctrl S'][data-shortcut*='click']").trigger("click");
        }
    });

    // Register plugin
    tinymce.PluginManager.add('RebelCmssave', tinymce.plugins.RebelCmsSave);
})();