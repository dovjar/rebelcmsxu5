(function () {
    tinymce.create('tinymce.plugins.RebelCmsLinkPlugin', {
        init: function (ed, url) {
            this.editor = ed;

            // Register commands
            ed.addCommand('mceRebelCmsLink', function () {
                var se = ed.selection;

                // No selection and not in link
                if (se.isCollapsed() && !ed.dom.getParent(se.getNode(), 'A'))
                    return;

                ed.windowManager.open({
                    file: tinyMCE.activeEditor.getParam('RebelCms_mce_controller_paths')['InsertLink'],
                    width: 480 + parseInt(ed.getLang('advlink.delta_width', 0)),
                    height: 510 + parseInt(ed.getLang('advlink.delta_height', 0)),
                    inline: 1
                }, {
                    plugin_url: url
                });
            });

            // Register buttons
            ed.addButton('RebelCmslink', {
                title: 'advlink.link_desc',
                cmd: 'mceRebelCmsLink'
            });

            ed.addShortcut('ctrl+k', 'advlink.advlink_desc', 'mceRebelCmsLink');

            ed.onNodeChange.add(function (ed, cm, n, co) {
                cm.setDisabled('RebelCmslink', co && n.nodeName != 'A');
                cm.setActive('RebelCmslink', n.nodeName == 'A' && !n.name);
            });
        },

        getInfo: function () {
            return {
                longname: 'RebelCms Link Plugin',
                author: 'RebelCms HQ',
                authorurl: 'http://RebelCms.com',
                infourl: 'http://RebelCms.com',
                version: '1.0'
            };
        }
    });

    // Register plugin
    tinymce.PluginManager.add('RebelCmslink', tinymce.plugins.RebelCmsLinkPlugin);
})();