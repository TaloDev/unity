namespace TaloGameServices
{
    public class StatsPutRequest
    {
        public float change;
        public int aliasId;

        public StatsPutRequest(float change)
        {
            this.change = change;
            aliasId = Talo.CurrentAlias.id;
        }
    }
}
