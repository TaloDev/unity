namespace TaloGameServices
{
    [System.Serializable]
    public class Group
    {
        public string id;
        public string name;
        public string description;
        public object[] rules;
        public string ruleMode;
        public bool membersVisible;
        public int count;
        public Player[] members;
        public string updatedAt;
    }
}
