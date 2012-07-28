(function () {
    tinymce.create('tinymce.plugins.RebelCharMapPlugin', {
        init: function (ed, url) {
            this.editor = ed;

            // Register commands
            ed.addCommand('mceRebelCharMap', function () {
                ed.windowManager.open({
                    file: tinyMCE.activeEditor.getParam('rebel_mce_controller_paths')['InsertChar'],
                    width: 525 + parseInt(ed.getLang('charmap.delta_width', 0)),
                    height: 218 + parseInt(ed.getLang('charmap.delta_height', 0)),
                    inline: 1
                }, {
                    plugin_url: url
                });
            });

            // Register buttons
            ed.addButton('rebelcharmap', {
                title: 'rebelcharmap.desc',
                cmd: 'mceRebelCharMap'
            });
        },

        getInfo: function () {
            return {
                longname: 'Rebel Char Map',
                author: 'Rebel HQ',
                authorurl: 'http://rebelcms.com',
                infourl: 'http://rebelcms.com',
                version: '1.0'
            };
        }
    });

    // Register plugin
    tinymce.PluginManager.add('rebelcharmap', tinymce.plugins.RebelCharMapPlugin);
})();
