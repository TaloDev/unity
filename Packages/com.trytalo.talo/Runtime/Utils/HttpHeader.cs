namespace TaloGameServices
{
    [System.Serializable]
    public class HttpHeader
    {
        public string key;
        public string value;

        public HttpHeader(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
