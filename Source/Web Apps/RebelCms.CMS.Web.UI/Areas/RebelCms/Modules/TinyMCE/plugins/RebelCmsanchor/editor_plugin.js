(function () {
    tinymce.create('tinymce.plugins.RebelCmsAnchorPlugin', {
        init: function (ed, url) {
            this.editor = ed;

            // Register commands
            ed.addCommand('mceRebelCmsAnchor', function () {
                ed.windowManager.open({
                    file: tinyMCE.activeEditor.getParam('RebelCms_mce_controller_paths')['InsertAnchor'], 
                    width: 390 + parseInt(ed.getLang('anchor.delta_width', 0)),
                    height: 100 + parseInt(ed.getLang('anchor.delta_height', 0)),
                    inline: 1
                }, {
                    plugin_url: url
                });
            });

            // Register buttons
            ed.addButton('RebelCmsanchor', {
                title: 'RebelCmsanchor.desc',
                cmd: 'mceRebelCmsAnchor'
            });
        },

        getInfo: function () {
            return {
                longname: 'RebelCms Anchor',
                author: 'RebelCms HQ',
                authorurl: 'http://RebelCms.com',
                infourl: 'http://RebelCms.com',
                version: '1.0'
            };
        }
    });

    // Register plugin
    tinymce.PluginManager.add('RebelCmsanchor', tinymce.plugins.RebelCmsAnchorPlugin);
})();