using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.PropertyEditors.Upload
{
    [PropertyEditor(CorePluginConstants.FileUploadPropertyEditorId, "Upload", "Upload", IsParameterEditor = true)]
    public class UploadEditor : ContentAwarePropertyEditor<UploadEditorModel, UploadPreValueModel>
    {
        protected IBackOfficeRequestContext BackOfficeRequestContext { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadEditor"/> class.
        /// </summary>
        /// <param name="backOfficeRequestContext">The back office request context.</param>
        public UploadEditor(IBackOfficeRequestContext backOfficeRequestContext)
        {
            BackOfficeRequestContext = backOfficeRequestContext;
        }

        /// <summary>
        /// Creates the editor model
        /// </summary>
        /// <param name="preValues"></param>
        /// <returns></returns>
        public override UploadEditorModel CreateEditorModel(UploadPreValueModel preValues)
        {
            var contentId = HiveId.Empty;

            try
            {
                contentId = GetContentModelValue<ProfileEditorModel, HiveId>(x => x.ProfileId, HiveId.Empty);
            }
            catch (InvalidCastException ex)
            {
                contentId = GetContentModelValue(x => x.Id, HiveId.Empty);
            }

            return new UploadEditorModel(preValues, BackOfficeRequestContext,
                contentId,
                GetContentPropertyValue(x => x.Alias, ""));
        }

        /// <summary>
        /// Creates the pre value model
        /// </summary>
        /// <returns></returns>
        public override UploadPreValueModel CreatePreValueEditorModel()
        {
            return new UploadPreValueModel();
        }

    }
}
