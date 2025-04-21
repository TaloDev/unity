namespace TaloGameServices
{
    [System.Serializable]
    public class PlayersMergeRequest
    {
        public string playerId1, playerId2;

        public PlayersMergeRequest(string playerId1, string playerId2)
        {
            this.playerId1 = playerId1;
            this.playerId2 = playerId2;
        }
    }
}
