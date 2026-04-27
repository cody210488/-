using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using 打球啊.Data;
using 打球啊.Models;

namespace 打球啊.Controllers
{
    public class PlayerProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlayerProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }
        //個人資料頁
        [Authorize]
        public async Task<IActionResult> MyProfile()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }
            var profile = await _context.PlayerProfiles
                .FirstOrDefaultAsync(p => p.UserId==userId );
            if (profile == null)
            {
                return RedirectToAction(nameof(Create));

            }
            return View(profile);
        }
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(PlayerProfile profile, IFormFile? photoFile)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }
            profile.UserId = userId;

            ModelState.Remove(nameof(PlayerProfile.UserId));

            bool exists = await _context.PlayerProfiles
                .AnyAsync(p => p.UserId == userId);
            if (exists)
            {
                TempData["Message"] = "你已建立過個人資料";
                return RedirectToAction(nameof(MyProfile));
            }
            if (!ModelState.IsValid)
            {
                return View(profile);
            }
            if (photoFile != null && photoFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photoFile.FileName);

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await photoFile.CopyToAsync(stream);
                }

                profile.Photo = "/images/" + fileName;
            }
            
            _context.PlayerProfiles.Add(profile);
            await _context.SaveChangesAsync();
            TempData["Message"] = "個人資料建立成功";
            return RedirectToAction(nameof(MyProfile));
        }

        [Authorize]
        public async Task<IActionResult> Edit()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }
            var profile=await _context.PlayerProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                return RedirectToAction(nameof(Create));
            }

            return View(profile);
        }
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Edit(PlayerProfile profile, IFormFile? photoFile)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var profileDb = await _context.PlayerProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profileDb == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(PlayerProfile.UserId));
            ModelState.Remove(nameof(PlayerProfile.Photo));

            if (!ModelState.IsValid)
            {
                // 很重要：回傳前把原本照片帶回去，不然畫面會像消失
                profile.Photo = profileDb.Photo;
                return View(profile);
            }

            if (photoFile != null && photoFile.Length > 0)
            {
                var ext = Path.GetExtension(photoFile.FileName).ToLower();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("", "只允許上傳 jpg、jpeg、png、webp 圖片");
                    profile.Photo = profileDb.Photo;
                    return View(profile);
                }
                if (!string.IsNullOrEmpty(profileDb.Photo))
                {
                    var oldPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        profileDb.Photo.TrimStart('/')
                    );

                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                var fileName = Guid.NewGuid().ToString() + ext;
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photoFile.CopyToAsync(stream);
                }

                profileDb.Photo = "/images/" + fileName;
            }

            profileDb.NickName = profile.NickName;
            profileDb.Age = profile.Age;
            profileDb.Height = profile.Height;
            profileDb.Weight = profile.Weight;
            profileDb.Position = profile.Position;
            profileDb.Skill = profile.Skill;
            profileDb.SkillLevel = profile.SkillLevel;
            profileDb.Introduction = profile.Introduction;

            await _context.SaveChangesAsync();

            TempData["Message"] = "個人資料更新成功";
            return RedirectToAction(nameof(MyProfile));
        }
        [Authorize]
        public async Task<IActionResult> Delete()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var profile = await _context.PlayerProfiles
                .FirstOrDefaultAsync(p => p.UserId==userId);
            if (profile==null)
            {
                return NotFound();
            }
            return View(profile);
        }
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }
            var profile = await _context.PlayerProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                return NotFound();
            }
            _context.PlayerProfiles.Remove(profile);
            await _context.SaveChangesAsync();
            TempData["Message"] = "個人資料已刪除";
            return RedirectToAction("Index", "Home");
        }

    }
}
