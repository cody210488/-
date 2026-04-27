namespace 打球啊.Models
{
    public class CourtComment
    {
        public int Id { get; set; }

        public int CourtId { get; set; }
        public Court Court { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public int Rating { get; set; } // 1~5

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ApplicationUser? User { get; set; }
    }
}
