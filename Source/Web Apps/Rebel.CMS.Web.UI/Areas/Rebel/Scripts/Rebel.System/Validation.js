/// <reference path="/Areas/Rebel/Scripts/Rebel.System/NamespaceManager.js" />


Rebel.System.registerNamespace("Rebel.System");

(function ($, Base) {


    Rebel.System.ValidationHelper = Base.extend(null, {
        ///<summary>A helper class for all things validation</summary>

        validateJsonResponse: function(response, ruleName) {
            ///<summary>This checks the json object to see if it matches our json validation error message format and if so displays the errors accordingly</summary>
            if (response == null || response.success == null || response.failureType == null || response.success == "true") return true;
            if (response.failureType == "ValidationError" && response.validationErrors != null) {
                var hasError = false;
                for (var v in response.validationErrors) {
                    hasError = true;
                    //creates a rule for the element found with the validation message found in the json response
                    var $element = $("#" + response.validationErrors[v].name);
                    if ($element.length > 0) {
                        //create a rule for each msg
                        for (var m in response.validationErrors[v].errors) {
                            var msgs = { };
                            msgs[ruleName] = response.validationErrors[v].errors[m];
                            var rule = { };
                            rule[ruleName] = true;
                            rule.messages = msgs;
                            //now we need to make sure the rule exist in the validation
                            if (typeof $.validator.methods[ruleName] == "undefined") {
                                $.validator.addMethod(ruleName,
                                    function(value, element) {
                                        return value == "true";
                                    });
                            }
                            $element.rules("add", rule);
                        }
                        //run the validation
                        $element.closest("form").valid();
                    }
                }
                return !hasError;
            }
            return true;
        }

    });


    $.fn.validationSummary = function(o) {
        ///<summary>Creates the Rebel validation summary and wires up the events for toggling, etc... </summary>

        return $(this).each(function() {

            var $this = $(this);
            var $content = $this.parent().find("#editorContent");
            //get initial top of tab content
            var originalTop = $content.position().top;
            var eventsWired = false;

            var api = {
                expandErrors: function($toggleBtn) {
                    $toggleBtn.removeClass("expand-button").addClass("collapse-button");
                    $toggleBtn.parent().find("ul").show();
                    var top = Number(originalTop + $this.outerHeight());
                    $content.attr("style", "top: "+ top + "px !important");
                },
                hideSummary: function() {
                    $content.attr("style", "top: "+ originalTop + "px");
                    $this.hide();
                },
                checkValidation: function() {
                    if (!$this.hasClass("valid")) {
                        $this.show();
                        //set the top to where it should be with the summary in place

                        //$content.attr("style", "top: "+ Number(originalTop + $this.outerHeight()) +"px !important");

                        this.expandErrors($this.find(".toggle-button"));

                        if (!eventsWired) {
                            $this.find(".toggle-button").click(function() {
                                if ($(this).hasClass("collapse-button")) {
                                    $(this).removeClass("collapse-button").addClass("expand-button");
                                    $this.find("ul").hide();
                                    $content.attr("style", "top: "+ Number(originalTop + $this.outerHeight()) + "px !important");
                                } else {
                                    $this.validationSummaryApi().expandErrors($(this));
                                }
                            });

                            eventsWired = true;
                        }
                    }
                }
            };

            //store the api in the object
            $this.data("api", api);

            //bind to the validation engine event
            $this.closest("form").bind("invalid-form.validate", function() {
                ///<summary>Binds to the jquery validation event when the form is invalidated</summary>
                
                //This is a horrible/awesome little hack to get jquery validation to tell us when the form is
                //actually successful since they don't actually give us this out of the box. 
                var form = $this.closest("form");
                if (form.data("hijackedValidation") != true) {                                        
                    var valOpts = form.data("validator");
                    //set our hijacked flag so we don't process again
                    form.data("hijackedValidation", true);
                    var f = valOpts.settings.success;
                    valOpts.settings.success = function(error) {
                        f.apply(form, [error]); //call the originally set method created by unobtrusive.
                        //now we can execute whatever we like!                        
                        //TODO: We need to figure out how to customize the validation summary so that
                        // each message in it refers to the field it belongs to, that way we can 
                        // dynamically remove each element succes.
                        // relates to issue: U5-249
                        if (valOpts.errorList.length == 0) {
                            api.hideSummary();
                        }
                    };
                }
                $this.removeClass("valid");
                api.checkValidation();
            });
            
            api.checkValidation();

        });
    };

    $.fn.validationSummaryApi = function(o) {
        ///<summary>Returns the api for the validation summary</summary>
        if ($(this).length != 1) {
            throw "validationSummaryApi selector requires that there be exactly one control selected, this selector returns " + $(this).length;
        }        
        return $(this).data("api");
	};

    $(window).load(function () {
        setTimeout(function() {
            
            //Not sure why but both IE and FF require a small delay otherwise an incorect originalTop value is recorded

            //initialize the validation summary
            $(".validation-summary").validationSummary();

            //set all property-editors to have the correct class when a sub property is invalid
            $(".field-validation-error").closest(".property-editor").addClass("invalidated");
            
        }, 100);
    });

})(jQuery, base2.Base);