(function () {
    tinymce.create('tinymce.plugins.RebelCmsMediaPlugin', {
        init: function (ed, url) {
            this.editor = ed;

            // Register commands
            ed.addCommand('mceRebelCmsMedia', function () {
                var se = ed.selection;

                ed.windowManager.open({
                    file: tinyMCE.activeEditor.getParam('RebelCms_mce_controller_paths')['InsertMedia'],
                    width: 645 + parseInt(ed.getLang('advlink.delta_width', 0)),
                    height: 460 + parseInt(ed.getLang('advlink.delta_height', 0)),
                    inline: 1
                }, {
                    plugin_url: url
                });
            });

            // Register buttons
            ed.addButton('RebelCmsmedia', {
                title: 'RebelCmsmedia.desc',
                cmd: 'mceRebelCmsMedia'
            });

        },

        getInfo: function () {
            return {
                longname: 'RebelCms Media Plugin',
                author: 'RebelCms HQ',
                authorurl: 'http://RebelCms.com',
                infourl: 'http://RebelCms.com',
                version: '1.0'
            };
        }
    });

    // Register plugin
    tinymce.PluginManager.add('RebelCmsmedia', tinymce.plugins.RebelCmsMediaPlugin);
})();