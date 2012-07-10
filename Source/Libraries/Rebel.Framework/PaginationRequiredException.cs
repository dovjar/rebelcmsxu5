using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Framework
{
    using System.Runtime.Serialization;

    public class PaginationRequiredException : Exception
    {
        public PaginationRequiredException() 
            : this("All results were requested, but no pagination was supplied (e.g. through using Skip and Take, or Paged methods). In order to be safe by default, queries cannot run unless a page size is provided. You can specify a default page size in the Hive configuration file.")
        {
        }

        public PaginationRequiredException(string message) : base(message)
        {
        }

        public PaginationRequiredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PaginationRequiredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }


    }
}
