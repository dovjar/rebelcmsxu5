Rebel.System.registerNamespace("Rebel.PropertyEditors");
(function ($, Base) {

    Rebel.PropertyEditors.NodeSelectorPreValues = Base.extend({

        //Private
        _viewModel: null,
        _opts: null,
        _el: null,

        _validateOptions: function (o) {
            ///<summary>Validates the options object</summary>
            var required = ["getPickerUrl", "startNodeIdFieldId", "startNodeIdFieldName", "$nodePickerPlaceholder", "startNodeSelectionType"];
            for (var i in required) {                
				if (typeof o[required[i]] == "undefined" || o[required[i]] == null) {
					throw "Required option " + required[i] + " for NodeSelectorPreValues have not been set";
				}                
            }
        },

        constructor: function (el, o) {
            ///<summary>Constructor</summary>

            this._el = el;

            var _this = this;
            var firstRun = true;
            
            //set optional props
            this._opts = $.extend({                
                savedSelectedTree: "",
                savedSelectedId: ""                
            }, o);

            //validate option params
            this._validateOptions(this._opts);

            this._viewModel = $.extend({}, Rebel.System.BaseViewModel, {
                // Always set parent                    
                parent: this,
                selectedTreeType: ko.observable(),
                startNodeSelectionType: ko.observable(this._opts.startNodeSelectionType)
            });

            this._viewModel.selectedTreeType.subscribe(function (data) {
                ///<summary>On change handler for the tree type drop down list</summary>

                //this method will fire on page load since the value of the view model is 'changed'
                //but we don't want to fire the ajax loading stuff just yet.
                if (firstRun) {
                    firstRun = false;
                    return;
                }

                if (data != null && data != "") {
                    
                    //we need to check if we are re-loading the tree that is saved, if so,
                    //we'll pass up the current selected id that is saved                     
                    var selectedId = data == _this._opts.savedSelectedTree ? _this._opts.savedSelectedId : null;

                    var postData = ko.toJSON({
                        TreeControllerId: data,
                        Id: _this._opts.startNodeIdFieldId,
                        Name: _this._opts.startNodeIdFieldName,
                        SelectedValue: selectedId
                    });

                    //clear modals created thus far, this is required because the tree-picker by default
                    //doesn't remove the moals from the DOM when they are closed and since we are loading a
                    //new tree-picker with the same ID, we will end up with 2 elements with the same ID, one of which
                    //the tree is probably loaded in already and therefore it will not load again in the new element with
                    //the duplicated html id.
                    $u.Sys.WindowManager.getInstance().removeModal();

                    //manually set this using jquery since we require a 'stop-binding' on this element
                    _this._opts.$nodePickerPlaceholder.html("Loading...");

                    $.post(_this._opts.getPickerUrl,
                            postData,
                            function (d) {
                                if (d.Success) {
                                    //manually set this using jquery since we require a 'stop-binding' on this element
                                    _this._opts.$nodePickerPlaceholder.html(d.Markup);
                                }
                                else {
                                    alert("An exception occurred: " + d.Exception);
                                }
                            });

                }
            });

            //knockout js apply bindings, scoped to the current tree picker
            ko.applyBindings(this._viewModel, el);
        }
    });

})(jQuery, base2.Base);