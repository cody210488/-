namespace 打球啊.ViewModel
{
    public class CourtCommentViewModel
    {
        public int Id { get; set; }
        public int CourtId { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string NickName { get; set; } = "匿名使用者";
        public string? Photo { get; set; }

        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}