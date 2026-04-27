using Microsoft.AspNetCore.Identity;

namespace 打球啊.Models
{
    public class ApplicationUser : IdentityUser
    {
        // 你可以自訂欄位
        public PlayerProfile? PlayerProfile { get; set; }
    }
}