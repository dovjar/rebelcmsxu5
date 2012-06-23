(function () {
    tinymce.create('tinymce.plugins.RebelCmsCharMapPlugin', {
        init: function (ed, url) {
            this.editor = ed;

            // Register commands
            ed.addCommand('mceRebelCmsCharMap', function () {
                ed.windowManager.open({
                    file: tinyMCE.activeEditor.getParam('RebelCms_mce_controller_paths')['InsertChar'],
                    width: 525 + parseInt(ed.getLang('charmap.delta_width', 0)),
                    height: 218 + parseInt(ed.getLang('charmap.delta_height', 0)),
                    inline: 1
                }, {
                    plugin_url: url
                });
            });

            // Register buttons
            ed.addButton('RebelCmscharmap', {
                title: 'RebelCmscharmap.desc',
                cmd: 'mceRebelCmsCharMap'
            });
        },

        getInfo: function () {
            return {
                longname: 'RebelCms Char Map',
                author: 'RebelCms HQ',
                authorurl: 'http://RebelCms.com',
                infourl: 'http://RebelCms.com',
                version: '1.0'
            };
        }
    });

    // Register plugin
    tinymce.PluginManager.add('RebelCmscharmap', tinymce.plugins.RebelCmsCharMapPlugin);
})();