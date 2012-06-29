using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security.Model.Entities;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;
using FixedHiveIds = Umbraco.Framework.Security.Model.FixedHiveIds;

namespace Umbraco.Cms.Web.PropertyEditors.UserGroupPicker
{
    [Bind(Exclude = "AvailableTypes")]
    [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.UserGroupPicker.Views.UserGroupPicker.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
    public class UserGroupPickerEditorModel : EditorModel<UserGroupPickerPreValueModel>, IValidatableObject
    {
        private IUmbracoApplicationContext _appContext;

        public UserGroupPickerEditorModel(IUmbracoApplicationContext appContext, UserGroupPickerPreValueModel preValues) 
            : base(preValues)
        {
            _appContext = appContext;
        }

        /// <summary>
        /// Gets or sets the value (the type alias).
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets the available types.
        /// </summary>
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public IEnumerable<SelectListItem> AvailableTypes
        {
            get
            {
                var availableTypes = Enumerable.Empty<SelectListItem>();

                using (var uow = _appContext.Hive.OpenReader<ISecurityStore>())
                {
                    var root = FixedHiveIds.MemberGroupVirtualRoot;

                    switch (PreValueModel.Type)
                    {
                        case UserGroupPickerType.Member:
                            root = FixedHiveIds.MemberGroupVirtualRoot;
                            break;
                        case UserGroupPickerType.User:
                            root = FixedHiveIds.UserGroupVirtualRoot;
                            break;
                    }

                    var userGroups =
                        uow.Repositories.GetChildren<UserGroup>(FixedRelationTypes.DefaultRelationType, root);

                    availableTypes = userGroups.Select(x => new SelectListItem {Value = x.Id.ToString(), Text = x.Name}).ToList();
                }

                var item = availableTypes.SingleOrDefault(x => x.Value == Value);
                if (item != null)
                    item.Selected = true;

                return availableTypes;
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PreValueModel.IsRequired && string.IsNullOrWhiteSpace(Value))
            {
                yield return new ValidationResult("Value is required", new[] { "Value" });
            }
        }
    }
}
