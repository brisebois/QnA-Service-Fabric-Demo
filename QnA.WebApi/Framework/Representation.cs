using Newtonsoft.Json.Linq;

namespace QnA.WebApi.Framework
{
    public class Representation
    {
        readonly JObject representation;
        readonly JArray links;

        private Representation()
        {
            representation = new JObject();
            links = new JArray();
            representation.Add(new JProperty("links", links));
        }

        public Representation AddValue(string name, string value)
        {
            representation.Add(new JProperty(name, value));
            return this;
        }
        public Representation AddValue(string name, int value)
        {
            representation.Add(new JProperty(name, value));
            return this;
        }

        public Representation AddLink(string relationName, string uri)
        {
            links.Add(new JObject(new JProperty("rel", relationName), new JProperty("uri", uri)));
            return this;
        }

        public JObject Json()
        {
            return representation;
        }

        public static Representation Make()
        {
            return new Representation();
        }

        public Representation AddArray(string name, object[] values)
        {
            representation.Add(new JProperty(name, new JArray(values)));
            return this;
        }

        public Representation AddValue(string name, JObject value)
        {
            representation.Add(new JProperty(name, value));
            return this;
        }
    }
}
