using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.UI.Models
{
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [UIHint("Password")]
        public string Password { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string ReturnUrl { get; set; }

        [HiddenInput(DisplayValue = false)]
        public HiveId DefaultRedirectId { get; set; }
    }
}