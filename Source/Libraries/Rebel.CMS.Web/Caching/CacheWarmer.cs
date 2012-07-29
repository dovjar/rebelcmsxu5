using System.IO;
using System.Net;
using System.Web.Script.Serialization;

namespace Rebel.Cms.Web.Caching
{
    public class CacheWarmer
    {
        private string Host { get; set; }

        public CacheWarmer(string host)
        {
            Host = host;
        }

        public void TraverseFrom(string node)
        {
            try
            {
                string url = string.Concat(Host, node);
                WebRequest request = WebRequest.Create(url + "?format=json");
                string text;
                var response = (HttpWebResponse)request.GetResponse();

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    text = sr.ReadToEnd();
                }

                var deserialised = new JavaScriptSerializer().Deserialize<DeserializedNode>(text);
                foreach (var childUrl in deserialised.children)
                {
                    TraverseFrom(childUrl);
                }

                request = WebRequest.Create(url);
                request.GetResponse();
            }
            catch {} // To have the cache warmed is a nice-to-have, if something goes wrong don't let the app fall over
        }
    }

    public class DeserializedNode
    {
        public string[] children { get; set; }
    }
}