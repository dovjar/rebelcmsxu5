Umbraco.System.registerNamespace("Umbraco.PropertyEditors");

//create a client side validators

$.validator.addMethod("nsMaxItems", function (value, element, param) {
    var n = Number(param);
    var v = !value ? 0 : (value == "" ? 0 : value.split(",").length);
    return n <= 0 || v <= n;
});
$.validator.addMethod("nsMinItems", function (value, element, param) {
    var n = Number(param);
    var v = !value ? 0 : (value == "" ? 0 : value.split(",").length);
    return n <= 0 || v >= n;
});
$.validator.unobtrusive.adapters.addSingleVal("nsMaxItems", "count");
$.validator.unobtrusive.adapters.addSingleVal("nsMinItems", "count");

//create a custom binding handler for the jquery sort opereation

ko.bindingHandlers.onNodeSelectorSort = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var callback = valueAccessor();
        $(element).bind("sortstop", function (event, ui) {
            var sortedElements = $(event.currentTarget).find(".item");
            callback.call(viewModel, sortedElements);
        });
    }
};

(function ($, Base) {

    Umbraco.PropertyEditors.NodeSelector = Base.extend({
        //private

        _viewModel: null,
        _opts: null,
        _el: null,
        _itemModelFromNode: function ($n) {
            ///<summary>Creates the item model from the jstree li node</summary>
            var hiveId = new Umbraco.System.HiveId($n.data().jsonId);
            var model = {
                nodeName: $n.find("span").html(),
                nodeStyle: $n.find("a ins").attr("class"),
                nodeIcon: $n.find("a ins").attr("style"),
                hiveId: hiveId,
                htmlId: hiveId.htmlId(),
                displayInfo: this._opts.preVals.showTooltip,
                selectable: (typeof $n.data().isSelectable != "undefined") ? $n.data().isSelectable : true
            };
            if (this._opts.preVals.showThumbnails) {
                //if we are showing thumbs, then we need to add the properties for the image
                model["isLoadingImage"] = ko.observable(true);
                model["hasImage"] = ko.observable(true);
                model["imageMarkup"] = ko.observable();
                //now go get the image url via ajax
                var data = ko.toJSON({ id: hiveId.rawValue(), treeId: this._opts.preVals.selectedTree, attributeAlias: this._opts.preVals.thumbnailPropertyName });
                $.post(this._opts.getMediaUrl, data, function (e) {
                    model["isLoadingImage"](false);
                    var hasImage = (e.url && e.url != "");
                    model["hasImage"](hasImage);
                    model["imageMarkup"]("");
                    if (hasImage) {
                        model["imageMarkup"]("<img src='" + e.url + "' />");
                    }
                });
            }
            return model;
        },
        _setupModelFromInitVals: function (initVals) {
            ///<summary>add init values to the ViewModel</summary>
            for (var i in initVals) {
                var hiveId = new Umbraco.System.HiveId(initVals[i].nodeId);
                var model = {
                    nodeName: initVals[i].nodeName,
                    nodeStyle: initVals[i].nodeStyle,
                    nodeIcon: (initVals[i].nodeIcon == null || initVals[i].nodeIcon == "") 
                        ? "" 
                        : ("background:url('" + initVals[i].nodeIcon + "') no-repeat;"),
                    hiveId: hiveId,
                    htmlId: hiveId.htmlId(),
                    displayInfo: this._opts.preVals.showTooltip
                };
                if (this._opts.preVals.showThumbnails) {
                    //if we are showing thumbs, then we need to add the properties for the image
                    model["isLoadingImage"] = ko.observable(false);
                    model["hasImage"] = ko.observable(initVals[i].thumbnailUrl && initVals[i].thumbnailUrl != "");
                    model["imageMarkup"] = "";
                    if (model["hasImage"]()) {
                        model["imageMarkup"] = "<img src='" + initVals[i].thumbnailUrl + "' />";
                    }
                }
                this._viewModel.selectedItems.push(model);
            }
        },
        _createTooltip: function () {
            ///<summary>Creates the tooltip for NodeSelector but only one of them</summary>
            if ($("#nsTooltip").length == 0) {
                //create the tooltip, only once!
                $("body").prepend("<div id='nsTooltip' class='jqmWindow'><div class='throbber'></div><div class='tooltipInfo'></div></div>");
            }
        },
        _validateOptions: function (o) {
            ///<summary>Validates that all of the required options are set</summary>                      
            var required = [{ obj: o, items: ["getPathUrl", "getTooltipUrl", "getMediaUrl", "getFilterUrl", "dataTypeId"] },
                        { obj: o.preVals, items: ["treeId"]}];
            for (var i in required) {
                var obj = required[i].obj;
                for (var v in required[i].vals) {
                    if (typeof obj[required[i].vals[v]] == "undefined" || obj[required[i].vals[v]] == null) {
                        throw "Required option " + obj[required[i].vals[v]] + " for NodeSelector have not been set";
                    }
                }
            }
        },
        _initSortableList: function () {
            //create the sortable list:
            this._el.find(".right.pane .inner-pane").sortable({
                handle: ".handle",
                axis: "y",
                containment: this._el.find(".right.pane"),
                start: function (event, ui) { /*$("#nsTooltip").jqmHide();*/ }
            });
        },
        _checkNodeFilter: function (hiveId, callback) {
            var data = ko.toJSON({ dataTypeId: this._opts.dataTypeId, nodeId: hiveId.rawValue(), treeId: this._opts.preVals.selectedTree });
            var _this = this;
            _this._viewModel.filterLoading(true);
            $.post(this._opts.getFilterUrl, data, function (e) {
                //TODO: add local caching for each hive id so if the same one is selected again we don't have to run the ajax call
                _this._viewModel.filterLoading(false);
                if (e.success) {
                    callback.apply(_this, [e.result]);
                }
                else {
                    window.__debug__("filter error: " + e.errorMsg, "NodeSelector", true);
                    Umbraco.System.NotificationManager.getInstance().showNotifications(e.notifications);
                    callback.apply(_this, [false]);
                }
            });
        },

        //public

        constructor: function (el, o) {
            ///<summary>Constructor</summary>

            this._el = el;
            //store the API(this) in the jquery data for retreival
            this._el.data("api", this);

            var _this = this;

            //set optional props

            this._opts = $.extend({
                preVals: {},
                initVal: []
            }, o);
            this._opts.preVals = $.extend({
                editorHeight: 290,
                minNodeCount: 0,
                maxNodeCount: 0,
                showThumbnails: false,
                showTooltip: true
            }, this._opts.preVals);

            //validate option params

            this._validateOptions(this._opts);

            //create the tooltip

            this._createTooltip();

            //setup the jquery sort

            this._initSortableList();

            //create knockout view model:

            this._viewModel = $.extend({}, Umbraco.System.BaseViewModel, {
                parent: this, // Always set parent
                height: ko.observable(_this._opts.preVals.editorHeight),
                selectedItems: ko.observableArray(),
                filterLoading: ko.observable(false),
                removeItem: function (item) {
                    ///<summary>Used to remove an item from the right pane</summary>
                    _this._viewModel.selectedItems.remove(item);
                },
                syncTree: function (item) {
                    ///<summary>This method will select the node on the left when the node on the right is clicked</summary>

                    //we need to go fetch the selected node's path from the server
                    var data = ko.toJSON({ id: item.hiveId.rawValue(), treeId: _this._opts.preVals.selectedTree });
                    $.post(_this._opts.getPathUrl, data, function (e) {
                        if (!$.isArray(e))
                            throw "The response returned from getPathUrl is invalid";
                        var treeApi = _this._el.find(".left .jstree-umbraco").umbracoTreeApi();
                        treeApi.syncNode(e[0], function (args) {
                            //make the node appear selected
                            args.closest(".tree-root").find("a").removeClass("jstree-clicked");
                            args.find("a:first").addClass("jstree-clicked");
                        }, "none");
                    });
                },
                showTooltip: function (item, e) {
                    //move tooltip element to the mouse and show
                    $("#nsTooltip")
                                .css("top", e.pageY)
                                .css("left", e.pageX - 15)
                                .jqm({
                                    modal: false,
                                    overlay: 100,
                                    onShow: function (h) {
                                        h.w.html("");
                                        h.w.show();
                                        //make the ajax call                                        
                                        var data = ko.toJSON({ id: item.hiveId.rawValue(), treeId: _this._opts.preVals.selectedTree });
                                        $.post(_this._opts.getTooltipUrl, data, function (result) {
                                            h.w.html(result.content);
                                            if (result.height != -1) {
                                                h.w.height(result.height);
                                            }
                                            if (result.width != -1) {
                                                h.w.width(result.width);
                                            }
                                        });
                                        var isOver = true;
                                        var timer = null;
                                        h.w.mouseenter(function () {
                                            isOver = true;
                                            if (timer) clearTimeout(timer);
                                        });
                                        h.w.mouseleave(function () {
                                            isOver = false;
                                            timer = setTimeout(function () {
                                                if (!isOver) {
                                                    $("#nsTooltip").jqmHide();
                                                }
                                            }, 500);
                                        });
                                    }
                                }).jqmShow();
                },
                handleSort: function (sortedItems) {
                    ///<summary>fired when the sort operation is completed, this updates our array the correct order</summary>
                    this.selectedItems.sort(function (left, right) {
                        var index1 = sortedItems.index(sortedItems.filter("[data-id='" + left.htmlId + "']").get(0));
                        var index2 = sortedItems.index(sortedItems.filter("[data-id='" + right.htmlId + "']").get(0));
                        return index1 < index2 ? -1 : 1;
                    });
                }
            });
            this._viewModel.paneHeight = ko.computed(function () {
                ///<summary>calculates the height of the each left/right pane</summary>
                return this.height() - 50;
            }, this._viewModel);
            this._viewModel.serializedValue = ko.computed(function () {
                ///<summary>serializes the value to csv to be stored</summary>
                var csv = "";
                for (var i in this.selectedItems()) {
                    var itemModel = this.selectedItems()[i];
                    csv += itemModel.hiveId.rawValue() + ",";
                }
                if (csv.endsWith(",")) {
                    csv = csv.substr(0, csv.length - 1);
                }
                return csv;
            }, this._viewModel);
            this._viewModel.serializedValue.subscribe(function (val) {
                ///<summary>This will revalidate the selected nodes each time the collection changes</summary>

                //we are doing a setTimeout here as a total hack! 'subscribe' fires before the DOM is updated but jquery
                //validation requires the DOM to be set, so we will revalidate in 200 ms which should be enough time for the DOM to be updated
                setTimeout(function () { _this._el.find("input[type='hidden']").valid(); }, 200);
            });

            this._setupModelFromInitVals(this._opts.initVal);

            //knockout js apply bindings, scoped to the current tree picker
            ko.applyBindings(this._viewModel, el.get(0));
        },

        nodeClicked: function (e, args) {
            ///<summary>Handles the node clicked</summary>

            var _this = this;
            
            //make it appear not selected
            var a = args.node.find("a:first");
            a.removeClass("jstree-clicked");
            var itemModel = this._itemModelFromNode(args.node);
            
            function checkSelectable(s) {
                if (!s) {
                    a.effect("highlight", { color: '#FBE3E4' }, 2000);
                }
                else {
                    _this._viewModel.selectedItems.push(itemModel);
                }
            }

            //validate the click, first check if this item is already in the or if it is not selectable
            var selectable = this._el.find(".right .item[data-id='" + itemModel.htmlId + "']").length == 0;
            //now check against the node data
            if (selectable) selectable = itemModel.selectable;
            //now check if there are too many selected
            if (this._opts.preVals.maxNodeCount > 0 && selectable) 
                selectable = this._el.find(".right .item").length < this._opts.preVals.maxNodeCount;
            //now check if we need to see if the node is selectable by applying a filter to it
            if (selectable && (this._opts.preVals.nodeFilter && this._opts.preVals.nodeFilter != "")) {
                this._checkNodeFilter(itemModel.hiveId, function (result) {
                    checkSelectable(result);
                });
            }
            else {
                checkSelectable(selectable);
            }
        }
    },
            {
                //static

                nodeClickHandler: function (e, args) {
                    ///<summary>
                    /// global method for handling node's clicked for any NodeSelector, 
                    /// this proxies the execution to the scoped NodeSelector for the current node.
                    ///</summary>
                    var selector = args.node.closest(".node-selector");
                    if (selector.length == 0) return;
                    var selectorApi = selector.nodeSelectorApi();
                    selectorApi.nodeClicked(e, args);
                }
            });

    //jquery extensions

    $.fn.nodeSelector = function (opts) {
        /// <summary>jQuery extension to create the node selector</summary>
        return this.each(function () {
            var ns = new Umbraco.PropertyEditors.NodeSelector($(this), opts);
        });
    };

    $.fn.nodeSelectorApi = function () {
        /// <summary>exposes the Node selecotr api for the selected object</summary>
        if ($(this).length != 1) {
            throw "nodeSelectorApi selector requires that there be exactly one control selected, this selector returns " + $(this).length;
        };
        return $(this).data("api");
    };

})(jQuery, base2.Base);