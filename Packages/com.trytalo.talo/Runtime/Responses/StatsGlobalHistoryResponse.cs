using System;

namespace TaloGameServices
{
    [Serializable]
    public struct GlobalValueMetrics
    {
        public float minValue;
        public float maxValue;
        public float medianValue;
        public float averageValue;
        public float averageChange;
    }

    [Serializable]
    public class StatsGlobalHistoryResponse
    {
        public PlayerStatSnapshot[] history;
        public GlobalValueMetrics globalValue;
        public int count;
        public int itemsPerPage;
        public bool isLastPage;
    }
}
