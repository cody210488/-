using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace 打球啊.Models
{
    public class EventParticipant
    {
        public int Id { get; set; }
        [Required]
        public int EventId { get; set; }
        [Required]
        public string UserId { get; set; } = string.Empty;
        public DateTime JoinTime { get; set; } = DateTime.Now;

        public Event? Event { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

    }
}
