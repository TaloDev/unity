namespace TaloGameServices
{
    [System.Serializable]
    public class Prop
    {
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
    }
}
