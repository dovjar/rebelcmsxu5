using System;
using System.Linq;
using AutoMapper;
using Rebel.Cms.Domain.BackOffice;
using Rebel.Cms.Domain.BackOffice.Trees;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Tasks;

namespace Rebel.Cms.Web.Packages
{
    /// <summary>
    /// Represents a PackageAction to execute
    /// </summary>
    public abstract class PackageAction : AbstractWebTask
    {
        protected PackageAction(IRoutableRequestContext applicationContext)
            : base(applicationContext)
        {
            //Locate the editor attribute
            var packageActionAttributes = GetType()
                .GetCustomAttributes(typeof(PackageActionAttribute), false)
                .OfType<PackageActionAttribute>();

            if (!packageActionAttributes.Any())
            {
                throw new InvalidOperationException("The PackageAction is missing the " + typeof(PackageActionAttribute).FullName + " attribute");
            }

            //assign the properties of this object to those of the metadata attribute
            var attr = packageActionAttributes.First();

            //set this objects properties
            Mapper.Map(attr, this);
        }

        /// <summary>
        /// The name of the package action
        /// </summary>
        public string Name { get; private set; }
    }
}