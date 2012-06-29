using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Security.Model.Schemas;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    public class ProfileEditorModel : BasicContentEditorModel
    {
        public HiveId ProfileId { get; set; }

        /// <summary>
        /// Override the 'Name' property to lookup/retreive from the Name dynamic property of this object
        /// </summary>
        [Required]
        public override string Name
        {
            get { return GetPropertyEditorModelValue(NodeNameAttributeDefinition.AliasValue, x => x.Name); }
            set { SetPropertyEditorModelValue(NodeNameAttributeDefinition.AliasValue, x => x.Name = value); }
        }

        /// <summary>
        /// Gets or sets the provider user key.
        /// </summary>
        /// <value>
        /// The provider user key.
        /// </value>
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public object ProviderUserKey
        {
            get { return GetPropertyEditorModelValue(ProfileSchema.ProviderUserKeyAlias, x => x.Value); }
            set { SetPropertyEditorModelValue(ProfileSchema.ProviderUserKeyAlias, x => x.Value = value == null ? null : value.ToString()); }
        }
    }
}
