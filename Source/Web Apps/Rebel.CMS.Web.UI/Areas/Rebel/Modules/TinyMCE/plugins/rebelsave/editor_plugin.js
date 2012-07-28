(function () {
    tinymce.create('tinymce.plugins.RebelSave', {
        init: function (ed, url) {
            var t = this;
            t.editor = ed;
            ed.addCommand('mceSave', t._save, t);
            ed.addShortcut('ctrl+s', ed.getLang('save.save_desc'), 'mceSave');
        },

        getInfo: function () {
            return {
                longname: 'Rebel Save',
                author: 'Rebel HQ',
                authorurl: 'http://rebelcms.com',
                infourl: 'http://rebelcms.com',
                version: "1.0"
            };
        },

        // Private methods
        _save: function () {
            // Just pass to the Rebel save button
            $("*[data-shortcut*='ctrl S'][data-shortcut*='click']").trigger("click");
        }
    });

    // Register plugin
    tinymce.PluginManager.add('rebelsave', tinymce.plugins.RebelSave);
})();
