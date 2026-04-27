using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 打球啊.Models
{
    public class PlayerProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string NickName { get; set; } = string.Empty;

        [Range(1, 100)]
        public int Age { get; set; }

        [Range(50, 250)]
        public int Height { get; set; }

        [Range(20, 300)]
        public int Weight { get; set; }

        [StringLength(30)]
        public string Position { get; set; } = string.Empty;

        [StringLength(30)]
        public string Skill { get; set; } = string.Empty;

        [Required]
        public SkillLevel SkillLevel { get; set; }

        [StringLength(500)]
        public string Introduction { get; set; } = string.Empty;

        public string? Photo { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}