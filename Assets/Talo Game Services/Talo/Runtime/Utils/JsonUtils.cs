using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace TaloGameServices
{
    public static class JsonUtils
    {
        public static string BuildObject(params (string key, object value)[] fields)
        {
            var parts = new List<string>();
            foreach (var (key, value) in fields)
            {
                if (value == null)
                {
                    continue;
                }
                parts.Add($"\"{key}\":{SerializeValue(value)}");
            }
            return "{" + string.Join(",", parts) + "}";
        }

        private static string SerializeValue(object value)
        {
            if (value is Prop p) return SerializeProp(p);
            if (value is Prop[] props) return "[" + string.Join(",", props.Select(SerializeProp)) + "]";
            if (value is string s) return JsonEscape(s);
            if (value is bool b) return b ? "true" : "false";
            if (value is int i) return i.ToString();
            if (value is long l) return l.ToString();
            if (value is float f) return f.ToString(CultureInfo.InvariantCulture);
            if (value is double d) return d.ToString(CultureInfo.InvariantCulture);
            return JsonUtility.ToJson(value);
        }

        private static string SerializeProp(Prop p) => Prop.SanitiseJson(JsonUtility.ToJson(p));

        [System.Serializable]
        private class JsonString { public string v; }

        private static string JsonEscape(string s)
        {
            // construct a json string and let JsonUtility handle the escaping
            var json = JsonUtility.ToJson(new JsonString { v = s });
            // strip {"v": prefix and trailing } from JsonUtility's {"v":"..."} output
            return json.Substring(5, json.Length - 6);
        }
    }
}
