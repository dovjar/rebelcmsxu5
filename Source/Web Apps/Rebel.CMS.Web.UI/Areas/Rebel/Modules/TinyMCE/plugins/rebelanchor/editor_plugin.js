(function () {
    tinymce.create('tinymce.plugins.RebelAnchorPlugin', {
        init: function (ed, url) {
            this.editor = ed;

            // Register commands
            ed.addCommand('mceRebelAnchor', function () {
                ed.windowManager.open({
                    file: tinyMCE.activeEditor.getParam('rebel_mce_controller_paths')['InsertAnchor'],
                    width: 390 + parseInt(ed.getLang('anchor.delta_width', 0)),
                    height: 100 + parseInt(ed.getLang('anchor.delta_height', 0)),
                    inline: 1
                }, {
                    plugin_url: url
                });
            });

            // Register buttons
            ed.addButton('rebelanchor', {
                title: 'rebelanchor.desc',
                cmd: 'mceRebelAnchor'
            });
        },

        getInfo: function () {
            return {
                longname: 'Rebel Anchor',
                author: 'Rebel HQ',
                authorurl: 'http://rebelcms.com',
                infourl: 'http://rebelcms.com',
                version: '1.0'
            };
        }
    });

    // Register plugin
    tinymce.PluginManager.add('rebelanchor', tinymce.plugins.RebelAnchorPlugin);
})();
