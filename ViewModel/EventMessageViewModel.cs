namespace 打球啊.ViewModel
{
    public class EventMessageViewModel
    {
        public int Id { get; set; }
        public int EventId { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string NickName { get; set; } = "匿名使用者";
        public string? Photo { get; set; }

        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
