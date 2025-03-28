namespace TaloGameServices
{
    [System.Serializable]
    public class EventsPostRequest
    {
        public Event[] events;

        public EventsPostRequest(Event[] events)
        {
            this.events = events;
        }
    }
}
