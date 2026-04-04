using System.Text.RegularExpressions;

namespace TaloGameServices
{
    [System.Serializable]
    public class Prop
    {
        public static string ToArrayKey(string key) {
            return key.EndsWith("[]") ? key : $"{key}[]";
        }

        public string key, value;

        public Prop((string, string) propTuple)
        {
            key = propTuple.Item1;
            value = propTuple.Item2;
        }

        public override string ToString()
        {
            return $"Key: {key}, Value: {value}";
        }

        public static string SanitiseJson(string json)
        {
            string match = "\"key\":\"([^\"]+)\",\"value\":\"\"";
            string replacement = "\"key\":\"$1\",\"value\":null";
            return Regex.Replace(json, match, replacement);
        }
    }
}
