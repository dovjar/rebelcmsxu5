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

namespace Umbraco.Cms.Web.PropertyEditors.TypePicker
{
    [Bind(Exclude = "AvailableTypes")]
    [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.TypePicker.Views.TypePicker.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
    public class TypePickerEditorModel : EditorModel<TypePickerPreValueModel>, IValidatableObject
    {
        private IUmbracoApplicationContext _appContext;

        public TypePickerEditorModel(IUmbracoApplicationContext appContext, TypePickerPreValueModel preValues) 
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

                switch (PreValueModel.Type)
                {
                    case TypePickerType.Member:
                        using(var uow = _appContext.Hive.OpenReader<ISecurityStore>())
                        {
                            var ids = uow.Repositories.Schemas.GetDescendantIds(FixedHiveIds.MasterMemberProfileSchema,
                                                                     FixedRelationTypes.DefaultRelationType);

                            availableTypes = 
                                uow.Repositories.Schemas.Get<EntitySchema>(true, ids).Select(
                                    x => new SelectListItem {Value = x.Alias, Text = x.Name}).ToList();

                        }
                        break;
                    case TypePickerType.Document:
                        using (var uow = _appContext.Hive.OpenReader<IContentStore>())
                        {
                            var ids = uow.Repositories.Schemas.GetDescendantIds(Umbraco.Framework.Persistence.Model.Constants.FixedHiveIds.ContentRootSchema,
                                                                     FixedRelationTypes.DefaultRelationType);

                            availableTypes = 
                                uow.Repositories.Schemas.Get<EntitySchema>(true, ids).Select(
                                    x => new SelectListItem { Value = x.Alias, Text = x.Name }).ToList();

                        }
                        break;
                    case TypePickerType.Media:
                        using (var uow = _appContext.Hive.OpenReader<IContentStore>())
                        {
                            var ids = uow.Repositories.Schemas.GetDescendantIds(Umbraco.Framework.Persistence.Model.Constants.FixedHiveIds.MediaRootSchema,
                                                                     FixedRelationTypes.DefaultRelationType);

                            availableTypes = 
                                uow.Repositories.Schemas.Get<EntitySchema>(true, ids).Select(
                                    x => new SelectListItem { Value = x.Alias, Text = x.Name }).ToList();

                        }
                        break;
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
