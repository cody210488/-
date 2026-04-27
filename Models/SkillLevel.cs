using System.ComponentModel.DataAnnotations;

namespace 打球啊.Models
{
    public enum SkillLevel
    {
        [Display(Name = "菜雞")] LowLevel,
        [Display(Name = "普普")] HigherLevel,
        [Display(Name = "中等")] MiddleLevel,
        [Display(Name = "高手")] TallLevel


    }
}
