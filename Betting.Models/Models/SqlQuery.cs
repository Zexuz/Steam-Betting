using System.Collections.Generic;

namespace Betting.Models.Models
{
    public class SqlQuery
    {
        public SqlQuery(string text, Dictionary<string, object> parameters)
        {
            Text       = text;
            Parameters = parameters;
        }

        public string                     Text       { get; }
        public Dictionary<string, object> Parameters { get; }

        public override string ToString()
        {
            var str = Text;
            foreach (var kvp in Parameters)
            {
                str = str.Replace(kvp.Key, kvp.Value.ToString());
            }

            return str;
        }
    }
}