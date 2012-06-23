using System;

namespace RebelCms.Framework
{
    public class UnreachableException : NotImplementedException
    {
        public UnreachableException()
            : base("This method is used for expression creation only")
        {
        }
    }
}