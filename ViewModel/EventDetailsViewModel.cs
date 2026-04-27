using 打球啊.Models;

namespace 打球啊.ViewModel
{
    public class EventDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }=string.Empty;
        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string CourtName { get; set; } = string.Empty;

        public int MaxParticipants {  get; set; }
        public int CurrentParticipants {  get; set; }
        public bool IsFull {  get; set; }
        public bool IsJoined {  get; set; }

        public string Description {  get; set; }= string.Empty;
        public string OrganizerName {  get; set; } = string.Empty;
        public SkillLevel SkillLevel { get; set; }

        public bool IsOrganizer {  get; set; }

    }
}
