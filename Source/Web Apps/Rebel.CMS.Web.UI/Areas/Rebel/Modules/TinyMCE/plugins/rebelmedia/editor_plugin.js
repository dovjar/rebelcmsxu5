(function () {
    tinymce.create('tinymce.plugins.RebelMediaPlugin', {
        init: function (ed, url) {
            this.editor = ed;

            // Register commands
            ed.addCommand('mceRebelMedia', function () {
                var se = ed.selection;

                ed.windowManager.open({
                    file: tinyMCE.activeEditor.getParam('rebel_mce_controller_paths')['InsertMedia'],
                    width: 645 + parseInt(ed.getLang('advlink.delta_width', 0)),
                    height: 460 + parseInt(ed.getLang('advlink.delta_height', 0)),
                    inline: 1
                }, {
                    plugin_url: url
                });
            });

            // Register buttons
            ed.addButton('rebelmedia', {
                title: 'rebelmedia.desc',
                cmd: 'mceRebelMedia'
            });

        },

        getInfo: function () {
            return {
                longname: 'Rebel Media Plugin',
                author: 'Rebel HQ',
                authorurl: 'http://rebelcms.com',
                infourl: 'http://rebelcms.com',
                version: '1.0'
            };
        }
    });

    // Register plugin
    tinymce.PluginManager.add('rebelmedia', tinymce.plugins.RebelMediaPlugin);
})();
