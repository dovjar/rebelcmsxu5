using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Rebel.Cms.Web.Model.BackOffice;

namespace Rebel.Cms.Web.PropertyEditors.ListPicker
{
    public class ListItem
    {
        [Required]
        [ShowLabel(false)]
        public string Id { get; set; }

        [Required]
        [ShowLabel(false)]
        public string Value { get; set; }
    }
}
