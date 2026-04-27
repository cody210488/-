using System.ComponentModel.DataAnnotations;
namespace 打球啊.Models
{
    public class Court
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;
        [Required]
        public string District { get; set; } = string.Empty;
        [Required]
        public string City { get; set; } = string.Empty;

        public string CourtType { get; set; } = string.Empty;
        public bool HasLighting { get; set; }
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        public List<Event> Events { get; set; } = new();
    }
}
