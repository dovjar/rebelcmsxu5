(function () {
    tinymce.create('tinymce.plugins.RebelLinkPlugin', {
        init: function (ed, url) {
            this.editor = ed;

            // Register commands
            ed.addCommand('mceRebelLink', function () {
                var se = ed.selection;

                // No selection and not in link
                if (se.isCollapsed() && !ed.dom.getParent(se.getNode(), 'A'))
                    return;

                ed.windowManager.open({
                    file: tinyMCE.activeEditor.getParam('rebel_mce_controller_paths')['InsertLink'],
                    width: 480 + parseInt(ed.getLang('advlink.delta_width', 0)),
                    height: 510 + parseInt(ed.getLang('advlink.delta_height', 0)),
                    inline: 1
                }, {
                    plugin_url: url
                });
            });

            // Register buttons
            ed.addButton('rebellink', {
                title: 'advlink.link_desc',
                cmd: 'mceRebelLink'
            });

            ed.addShortcut('ctrl+k', 'advlink.advlink_desc', 'mceRebelLink');

            ed.onNodeChange.add(function (ed, cm, n, co) {
                cm.setDisabled('rebellink', co && n.nodeName != 'A');
                cm.setActive('rebellink', n.nodeName == 'A' && !n.name);
            });
        },

        getInfo: function () {
            return {
                longname: 'Rebel Link Plugin',
                author: 'Rebel HQ',
                authorurl: 'http://rebelcms.com',
                infourl: 'http://rebelcms.com',
                version: '1.0'
            };
        }
    });

    // Register plugin
    tinymce.PluginManager.add('rebellink', tinymce.plugins.RebelLinkPlugin);
})();
