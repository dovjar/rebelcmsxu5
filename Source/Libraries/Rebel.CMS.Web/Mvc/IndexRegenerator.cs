using System.IO;
using System.Net;
using System.Web.Script.Serialization;

namespace Rebel.Cms.Web.Mvc
{
    public class IndexRegenerator
    {
        private string Host { get; set; }

        public IndexRegenerator(string host)
        {
            Host = host;
        }

        public void Go(string node)
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
                    Go(childUrl);
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