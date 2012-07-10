using System.Collections.Generic;
using System.Text;

namespace Rebel.Lucene
{
    using System.Security.AccessControl;

    public class IndexConfiguration
    {
        public IndexConfiguration(string buildLocation)
        {
            BuildLocation = buildLocation;
        }

        public string BuildLocation { get; protected set; }
    }
}
