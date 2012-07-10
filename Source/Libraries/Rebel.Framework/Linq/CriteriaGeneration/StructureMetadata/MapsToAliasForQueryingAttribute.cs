using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Framework.Linq.CriteriaGeneration.StructureMetadata
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MapsToAliasForQueryingAttribute : Attribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MapsToInnerAliasForQueryingAttribute : Attribute
    {

    }
}
