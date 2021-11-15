namespace TaloGameServices
{
    public class LeaderboardsPostRequest
    {
        public float score;
        public int aliasId;

        public LeaderboardsPostRequest(float score)
        {
            this.score = score;
            aliasId = Talo.CurrentAlias.id;
        }
    }
}
