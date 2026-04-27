using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 打球啊.Models
{
    public class Event
    {

        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string OrganizerUserId { get; set; } = string.Empty;
        [Required]
        public int CourtId { get; set; }
        // 活動日期
        [Required]
        public DateTime EventDate { get; set; }

        // 開始時間
        [Required]
        public TimeSpan StartTime { get; set; }

        // 結束時間
        [Required]
        public TimeSpan EndTime { get; set; }
        [Range(2, 30)]
        public int MaxPlayers { get; set; }


        public SkillLevel SkillLevel { get; set; }
        [StringLength(50)]
        public string Status { get; set; } = "開放報名";
        
        // 活動備註
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        [ForeignKey("CourtId")]
        public Court? Court { get; set; }
        public List<EventParticipant> Participants { get; set; } = new();
    }
}
