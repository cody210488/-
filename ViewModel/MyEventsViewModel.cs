namespace 打球啊.ViewModel
{
    public class MyEventsViewModel
    {
        public List<EventItem> CreatedEvents { get; set; } = new();
        public List<EventItem> JoinedEvents { get; set; } = new();
    }

    public class EventItem
    {
        public int Id { get; set; }
        public string Title {  get; set; }=string.Empty;
        public string CourtName { get; set; }=string.Empty ;

        public DateTime EventDate { get; set; }
        public int MaxPlayers {  get; set; }
        public int CurrentCount {  get; set; }
        public bool IsFull {  get; set; }
        public bool IsJoined { get; set; }
        public bool IsOwner { get; set; }
    }
    
}
