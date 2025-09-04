namespace TaloGameServices
{
    [System.Serializable]
    public class MembersPagination
    {
        public int count;
        public int itemsPerPage;
        public bool isLastPage;
    }

    [System.Serializable]
    public class PlayerGroupsGetResponse
    {
        public Group group;
        public MembersPagination membersPagination;
    }
}
