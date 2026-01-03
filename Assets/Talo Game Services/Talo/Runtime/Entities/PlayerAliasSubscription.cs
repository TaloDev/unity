using System;

namespace TaloGameServices
{
    public enum RelationshipType
    {
        Unidirectional,
        Bidirectional
    }

    [Serializable]
    public class PlayerAliasSubscription
    {
        public int id;
        public PlayerAlias subscriber;
        public PlayerAlias subscribedTo;
        public bool confirmed;
        public string relationshipType;
        public string createdAt;
        public string updatedAt;

        public RelationshipType GetRelationshipType()
        {
            return ParseRelationshipType(relationshipType);
        }

        public static RelationshipType ParseRelationshipType(string type)
        {
            return type == "bidirectional" ? RelationshipType.Bidirectional : RelationshipType.Unidirectional;
        }

        public static string RelationshipTypeToString(RelationshipType type)
        {
            return type == RelationshipType.Bidirectional ? "bidirectional" : "unidirectional";
        }
    }
}
