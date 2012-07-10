using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Framework.Localization.Configuration;
using Rebel.Framework.Localization.Web.Mvc;

namespace TestPlugin
{
    public class FancyStuff
    {

        public string SayHello()
        {
            return LocalizationConfig.TextManager.Get<FancyStuff>("Hello");
        }

        public string GetTulips()
        {
            return ResourceHelper.GetUrl<FancyStuff>("Tulips1");
        }

        public string GetMoreTulips()
        {
            return ResourceHelper.GetUrl<FancyStuff>("Tulips2");
        }
    }
}
