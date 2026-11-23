namespace TaloGameServices
{
    [System.Serializable]
    public class PlayersMergeRequest
    {
        public string playerId1, playerId2, player2ClaimKey;

        public PlayersMergeRequest(string playerId1, string playerId2, string player2ClaimKey = "")
        {
            this.playerId1 = playerId1;
            this.playerId2 = playerId2;
            this.player2ClaimKey = player2ClaimKey;
        }

        public string ToJson()
        {
            return JsonUtils.BuildObject(
                ("playerId1", playerId1),
                ("playerId2", playerId2),
                ("player2ClaimKey", string.IsNullOrEmpty(player2ClaimKey) ? null : player2ClaimKey)
            );
        }
    }
}
