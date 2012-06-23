using System;

namespace RebelCms.Framework.Persistence.ModelFirst.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AttributeAliasAttribute : Attribute
    {
        public string Alias { get; set; }
    }
}
