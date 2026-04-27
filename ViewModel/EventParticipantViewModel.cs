using 打球啊.Models;

namespace 打球啊.ViewModel
{
    public class EventParticipantViewModel
    {
        public string UserId { get; set; } = string.Empty;

        public string NickName { get; set; } = "匿名使用者";
        public string? Photo { get; set; }

        public int Age { get; set; }
        public SkillLevel SkillLevel { get; set; }
    }
}