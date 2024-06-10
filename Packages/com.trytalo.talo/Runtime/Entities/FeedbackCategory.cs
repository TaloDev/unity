using System;

namespace TaloGameServices
{
    [Serializable]
    public class FeedbackCategory
    {
        public int id;
        public string internalName;
        public string name;
        public string description;
        public bool anonymised;
        public string createdAt;
        public string updatedAt;
    }
}
