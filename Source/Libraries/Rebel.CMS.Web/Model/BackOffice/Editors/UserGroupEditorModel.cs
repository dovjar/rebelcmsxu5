﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Rebel.Cms.Web.Model.BackOffice.UIElements;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Constants.Schemas;

namespace Rebel.Cms.Web.Model.BackOffice.Editors
{
    public class UserGroupEditorModel : BasicContentEditorModel
    {

        /// <summary>
        /// Parent is always the UserVirtualRoot
        /// </summary>
        public override HiveId ParentId { get; set; }

        /// <summary>
        /// Override the 'Name' property to lookup/retreive from the Name dynamic property of this object
        /// </summary>
        [Required]
        public override string Name
        {
            get { return GetPropertyEditorModelValue(NodeNameAttributeDefinition.AliasValue, x => x.Value); }
            set { SetPropertyEditorModelValue(NodeNameAttributeDefinition.AliasValue, x => x.Value = value); }
        }

        /// <summary>
        /// Gets or sets the permissions.
        /// </summary>
        /// <value>
        /// The permissions.
        /// </value>
        public IEnumerable<PermissionStatusModel> Permissions { get; set; }

        protected override void PopulateUIElements()
        {
            UIElements.Clear();
            UIElements.Add(new SaveButtonUIElement());
        }
    }
}
