using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using System.Web.Mvc;
using System.Web;
using System.ComponentModel;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;
using File = Umbraco.Framework.Persistence.Model.IO.File;
using Umbraco.Hive;

namespace Umbraco.Cms.Web.PropertyEditors.Upload
{
    [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.Upload.Views.UploadEditor.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
    [ModelBinder(typeof(UploadEditorModelBinder))]
    [Bind(Exclude = "ContentId,PropertyAlias,File")]
    public class UploadEditorModel : EditorModel<UploadPreValueModel>, IValidatableObject
    {
        private readonly IBackOfficeRequestContext _backOfficeRequestContext;
        private readonly GroupUnitFactory<IFileStore> _hive;
        private readonly HiveId _contentId;
        private readonly string _propertyAlias;

        private File _file;

        public UploadEditorModel(UploadPreValueModel preValueModel, IBackOfficeRequestContext backOfficeRequestContext, 
            HiveId contentId, string propertyAlias)
            : base(preValueModel)
        {
            _backOfficeRequestContext = backOfficeRequestContext;
            _hive = _backOfficeRequestContext.Application.Hive.GetWriter<IFileStore>(new Uri("storage://file-uploader"));
            _contentId = contentId;
            _propertyAlias = propertyAlias;
        }

        [ScaffoldColumn(false)]
        [ReadOnly(true)]
        public HiveId ContentId
        {
            get { return _contentId;  }
        }

        [ScaffoldColumn(false)]
        [ReadOnly(true)]
        public string PropertyAlias
        {
            get { return _propertyAlias; }
        }

        /// <summary>
        /// The media value
        /// </summary>
        [ScaffoldColumn(false)]
        [ReadOnly(true)]
        public File File
        {
            get
            {
                if (Value.IsNullValueOrEmpty())
                    return null;

                if (_file == null)
                {
                    using (var uow = _hive.Create())
                    {
                        _file = uow.Repositories.Get<File>(Value);
                    }
                }

                return _file;
            }
        }

        /// <summary>
        /// A Guid that is used to track which folder to store the media in, when new media is created this model generates this Id which gets stored in the repository.
        /// </summary>
        public Guid MediaId { get; set; }

        /// <summary>
        /// Gets or sets the new file.
        /// </summary>
        public HttpPostedFileBase NewFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to remove file.
        /// </summary>
        [DisplayName("Remove file")]
        public bool RemoveFile { get; set; }

        /// <summary>
        /// The media value
        /// </summary>
        public HiveId Value { get; set; }

        public override void SetModelValues(IDictionary<string, object> serializedVal)
        {
            if (serializedVal.ContainsKey("MediaId"))
            {
                MediaId = Guid.Parse(serializedVal["MediaId"].ToString());
            }
            if (serializedVal.ContainsKey("Value"))
            {
                Value = HiveId.Parse((string) serializedVal["Value"]);
            }
        }

        public override IDictionary<string, object> GetSerializedValue()
        {
            //generate an id if we need one
            if (MediaId == Guid.Empty)
            {
                MediaId = Guid.NewGuid();
            }

            return ContentExtensions.WriteUploadedFile(MediaId, RemoveFile, NewFile, _hive, Value, PreValueModel.Sizes);
                        var img = Image.FromStream(NewFile.InputStream);
        }

        /// <summary>
        /// Executes custom server side validation for the model
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PreValueModel.IsRequired && Value.IsNullValueOrEmpty() && !ContentExtensions.HasFile(NewFile))
            {
                yield return new ValidationResult("Value is required", new[] { "Value" });
            }
        }
    }
}
