using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 打球啊.Data;
using 打球啊.Models;

namespace 打球啊.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CourtsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourtsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string searchKeyword, string searchCity, string searchDistrict, bool? hasLighting)
        {
            var courts = _context.Courts.AsQueryable();



            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                //嚴格搜尋
                //courts = courts.Where(c => c.City == searchCity);
                searchKeyword = searchKeyword.Trim();
                //模糊搜尋

                courts = courts.Where(c => c.City.Contains(searchKeyword) || c.District.Contains(searchKeyword)
                || c.Address.Contains(searchKeyword) || c.Name.Contains(searchKeyword)
                );
            }
            if (!string.IsNullOrWhiteSpace(searchCity))
            {
                courts = courts.Where(c => c.City == searchCity);
            }
            if (!string.IsNullOrWhiteSpace(searchDistrict))
            {
                courts = courts.Where(c => c.District == searchDistrict);
            }
            if (hasLighting == true)
            {
                courts = courts.Where(c => c.HasLighting == true);
            }
            //保留輸入的值
            ViewBag.SearchCIty = searchCity;
            ViewBag.SearchDistrict = searchDistrict;
            ViewBag.SearchKeyword = searchKeyword;
            ViewBag.HasLighting = hasLighting;

            var result = await courts.ToListAsync();
            return View(result);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Court court)
        {
            if (!ModelState.IsValid)
            {
                return View(court);
            }
            _context.Courts.Add(court);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courtId = id.Value;

            var court = await _context.Courts
                .FirstOrDefaultAsync(c => c.Id == courtId);

            if (court == null)
            {
                return NotFound();
            }

            var comments = await _context.CourtComments
                .Where(c => c.CourtId == courtId)
                .GroupJoin(
                _context.PlayerProfiles,
                c => c.UserId,
                p => p.UserId,
                (c, profiles) => new 打球啊.ViewModel.CourtCommentViewModel
                {
                    Id = c.Id,
                    CourtId = c.CourtId,
                    UserId = c.UserId,
                    Content = c.Content,
                    Rating = c.Rating,
                    CreatedAt = c.CreatedAt,
                    NickName = profiles.Select(p => p.NickName).FirstOrDefault() ?? "匿名使用者",
                    Photo = profiles.Select(p => p.Photo).FirstOrDefault()

                })
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();



            ViewBag.Comments = comments;

            return View(court);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }
            var court = await _context.Courts.FirstOrDefaultAsync(c => c.Id == id);
            if (court == null)
            {
                return NotFound();
            }
            return View(court);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Court court)
        {
            if (id != court.Id)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                return View(court);
            }
            //_context.Courts.Update(court);
            //await _context.SaveChangesAsync();
            var DBcourt = await _context.Courts.FirstOrDefaultAsync(c => c.Id == id);
            if (DBcourt == null)
            {
                return NotFound();
            }
            DBcourt.Name = court.Name;
            DBcourt.District = court.District;
            DBcourt.City = court.City;
            DBcourt.HasLighting = court.HasLighting;
            DBcourt.Description = court.Description;
            DBcourt.CourtType = court.CourtType;
            DBcourt.Address = court.Address;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var court = await _context.Courts.FirstOrDefaultAsync(c => c.Id == id);
            if (court == null)
            {
                return NotFound();
            }
            return View(court);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var court = await _context.Courts.FindAsync(id);
            if (court != null)
            {
                _context.Courts.Remove(court);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));


        }

        //回傳某個城市的所有地區
        [HttpGet]
        public IActionResult GetDistricts(string city)
        {
            var data = new Dictionary<string, List<string>>
            {
                { "台北市", new List<string> { "中正區","大同區","中山區","松山區","大安區","萬華區","信義區","士林區","北投區","內湖區","南港區","文山區" } },
                { "新北市", new List<string> { "板橋區","三重區","中和區","永和區","新莊區","新店區","樹林區","鶯歌區","三峽區","淡水區","汐止區","瑞芳區","土城區","蘆洲區","五股區","泰山區","林口區","深坑區","石碇區","坪林區","三芝區","石門區","八里區","平溪區","雙溪區","貢寮區","金山區","萬里區","烏來區" } },
                { "桃園市", new List<string> { "桃園區","中壢區","平鎮區","八德區","楊梅區","蘆竹區","大溪區","龍潭區","龜山區","大園區","觀音區","新屋區","復興區" } },
                { "台中市", new List<string> { "中區","東區","南區","西區","北區","西屯區","南屯區","北屯區","豐原區","東勢區","大甲區","清水區","沙鹿區","梧棲區","后里區","神岡區","潭子區","大雅區","新社區","石岡區","外埔區","大安區","烏日區","大肚區","龍井區","霧峰區","太平區","大里區","和平區" } },
                { "台南市", new List<string> { "中西區","東區","南區","北區","安平區","安南區","永康區","歸仁區","新化區","左鎮區","玉井區","楠西區","南化區","仁德區","關廟區","龍崎區","官田區","麻豆區","佳里區","西港區","七股區","將軍區","學甲區","北門區","新營區","後壁區","白河區","東山區","六甲區","下營區","柳營區","鹽水區","善化區","大內區","山上區","新市區","安定區" } },
                { "高雄市", new List<string> { "新興區","前金區","苓雅區","鹽埕區","鼓山區","旗津區","前鎮區","三民區","楠梓區","小港區","左營區","仁武區","大社區","岡山區","路竹區","阿蓮區","田寮區","燕巢區","橋頭區","梓官區","彌陀區","永安區","湖內區","鳳山區","大寮區","林園區","鳥松區","大樹區","旗山區","美濃區","六龜區","內門區","杉林區","甲仙區","桃源區","那瑪夏區","茂林區","茄萣區" } },
                { "基隆市", new List<string> { "仁愛區","信義區","中正區","中山區","安樂區","暖暖區","七堵區" } },
                { "新竹市", new List<string> { "東區","北區","香山區" } },
                { "嘉義市", new List<string> { "東區","西區" } },
                { "新竹縣", new List<string> { "竹北市","竹東鎮","新埔鎮","關西鎮","湖口鄉","新豐鄉","芎林鄉","橫山鄉","北埔鄉","寶山鄉","峨眉鄉","尖石鄉","五峰鄉" } },
                { "苗栗縣", new List<string> { "苗栗市","苑裡鎮","通霄鎮","竹南鎮","頭份市","後龍鎮","卓蘭鎮","大湖鄉","公館鄉","銅鑼鄉","南庄鄉","頭屋鄉","三義鄉","西湖鄉","造橋鄉","三灣鄉","獅潭鄉","泰安鄉" } },
                { "彰化縣", new List<string> { "彰化市","鹿港鎮","和美鎮","線西鄉","伸港鄉","福興鄉","秀水鄉","花壇鄉","芬園鄉","員林市","溪湖鎮","田中鎮","大村鄉","埔鹽鄉","埔心鄉","永靖鄉","社頭鄉","二水鄉","北斗鎮","二林鎮","田尾鄉","埤頭鄉","芳苑鄉","大城鄉","竹塘鄉","溪州鄉" } },
                { "南投縣", new List<string> { "南投市","埔里鎮","草屯鎮","竹山鎮","集集鎮","名間鄉","鹿谷鄉","中寮鄉","魚池鄉","國姓鄉","水里鄉","信義鄉","仁愛鄉" } },
                { "雲林縣", new List<string> { "斗六市","斗南鎮","虎尾鎮","西螺鎮","土庫鎮","北港鎮","古坑鄉","大埤鄉","莿桐鄉","林內鄉","二崙鄉","崙背鄉","麥寮鄉","東勢鄉","褒忠鄉","臺西鄉","元長鄉","四湖鄉","口湖鄉","水林鄉" } },
                { "嘉義縣", new List<string> { "太保市","朴子市","布袋鎮","大林鎮","民雄鄉","溪口鄉","新港鄉","六腳鄉","東石鄉","義竹鄉","鹿草鄉","水上鄉","中埔鄉","竹崎鄉","梅山鄉","番路鄉","大埔鄉","阿里山鄉" } },
                { "屏東縣", new List<string> { "屏東市","潮州鎮","東港鎮","恆春鎮","萬丹鄉","長治鄉","麟洛鄉","九如鄉","里港鄉","鹽埔鄉","高樹鄉","萬巒鄉","內埔鄉","竹田鄉","新埤鄉","枋寮鄉","新園鄉","崁頂鄉","林邊鄉","南州鄉","佳冬鄉","琉球鄉","車城鄉","滿州鄉","枋山鄉","三地門鄉","霧臺鄉","瑪家鄉","泰武鄉","來義鄉","春日鄉","獅子鄉","牡丹鄉" } },
                { "宜蘭縣", new List<string> { "宜蘭市","羅東鎮","蘇澳鎮","頭城鎮","礁溪鄉","壯圍鄉","員山鄉","冬山鄉","五結鄉","三星鄉","大同鄉","南澳鄉" } },
                { "花蓮縣", new List<string> { "花蓮市","鳳林鎮","玉里鎮","新城鄉","吉安鄉","壽豐鄉","光復鄉","豐濱鄉","瑞穗鄉","富里鄉","秀林鄉","萬榮鄉","卓溪鄉" } },
                { "台東縣", new List<string> { "台東市","成功鎮","關山鎮","卑南鄉","鹿野鄉","池上鄉","東河鄉","長濱鄉","太麻里鄉","大武鄉","綠島鄉","海端鄉","延平鄉","金峰鄉","達仁鄉","蘭嶼鄉" } },
                { "澎湖縣", new List<string> { "馬公市","湖西鄉","白沙鄉","西嶼鄉","望安鄉","七美鄉" } },
                { "金門縣", new List<string> { "金城鎮","金湖鎮","金沙鎮","金寧鄉","烈嶼鄉","烏坵鄉" } },
                { "連江縣", new List<string> { "南竿鄉","北竿鄉","莒光鄉","東引鄉" } }
            };

            if (!string.IsNullOrWhiteSpace(city) && data.ContainsKey(city))
            {
                return Json(data[city]);
            }
            return Json(new List<string>());
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int courtId, string content, int rating)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Message"] = "評論內容不能空白";
                return RedirectToAction("Details", new { id = courtId });
            }

            if (rating < 1 || rating > 5)
            {
                TempData["Message"] = "評分必須介於 1 到 5 之間";
                return RedirectToAction("Details", new { id = courtId });
            }

            var comment = new CourtComment
            {
                CourtId = courtId,
                Content = content.Trim(),
                Rating = rating,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            _context.CourtComments.Add(comment);
            await _context.SaveChangesAsync();

            TempData["Message"] = "評論新增成功";
            return RedirectToAction("Details", new { id = courtId });
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var comment = await _context.CourtComments.FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound();
            }

            if (comment.UserId != userId)
            {
                return Forbid();
            }

            var courtId = comment.CourtId;

            _context.CourtComments.Remove(comment);
            await _context.SaveChangesAsync();

            TempData["Message"] = "評論已刪除";
            return RedirectToAction("Details", new { id = courtId });
        }

    }
}
