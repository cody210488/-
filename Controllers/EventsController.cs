using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using 打球啊.Data;
using 打球啊.Models;
using 打球啊.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using 打球啊.Hubs;
namespace 打球啊.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        private readonly IHubContext<EventChatHub> _hubContext;

        public EventsController(ApplicationDbContext context, IHubContext<EventChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        
        [Authorize]
        public async Task<IActionResult> Create(int? courtId)
        {
            ViewBag.CourtList = new SelectList(
                await _context.Courts.ToListAsync(),
                "Id","Name", courtId);
            var ev = new Event();

            if (courtId.HasValue)
            {
                ev.CourtId = courtId.Value;
            }
            ev.EventDate = DateTime.Today;
            ev.StartTime=DateTime.Now.TimeOfDay;
            ev.EndTime = DateTime.Now.AddHours(2).TimeOfDay;
            return View(ev);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Event ev)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }
            
            ev.OrganizerUserId= userId;

            ModelState.Remove(nameof(Event.OrganizerUserId));
            if(ev.EndTime <= ev.StartTime)
            {
                ModelState.AddModelError(nameof(Event.EndTime),"結束時間必晚於開始間");
            }
            DateTime eventStartDateTime = ev.EventDate.Date.Add(ev.StartTime);

            if(eventStartDateTime < DateTime.Now)
            {
                ModelState.AddModelError(nameof(Event.StartTime),"活動開始時間不能早於現在時間");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CourtList = new SelectList(
                    await _context.Courts.ToListAsync(),
                    "Id", "Name", ev.CourtId);

                return View(ev);
            }
            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return RedirectToAction("ByCourt", new { courtId = ev.CourtId });
            
        }

        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Include(e => e.Court)
                .ToListAsync();

            return View(events);
        }

        public async Task<IActionResult> Details(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Court)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
            {
                return NotFound();
            }

            var participantCount = await _context.EventParticipants
                .CountAsync(p => p.EventId == id);

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            bool isJoined = false;
            if (!string.IsNullOrEmpty(userId))
            {
                isJoined = await _context.EventParticipants
                    .AnyAsync(p => p.EventId == id && p.UserId == userId);
            }

            string organizerName = "未知使用者";
            var organizer = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == ev.OrganizerUserId);

            if (organizer != null)
            {
                organizerName = organizer.UserName ?? organizer.Email ?? "未知使用者";
            }

            var vm = new EventDetailsViewModel
            {
                Id = ev.Id,
                Title = ev.Title,
                EventDate = ev.EventDate,
                StartTime = ev.StartTime,
                EndTime = ev.EndTime,
                CourtName = ev.Court?.Name ?? "",
                MaxParticipants = ev.MaxPlayers,
                CurrentParticipants = participantCount,
                IsFull = participantCount >= ev.MaxPlayers,
                IsJoined = isJoined,
                IsOrganizer = userId == ev.OrganizerUserId,
                Description = ev.Description,
                OrganizerName = organizerName,
                SkillLevel = ev.SkillLevel
            };
            var messages = await _context.EventMessages
                .Where(m => m.EventId == id)
                .GroupJoin(
                _context.PlayerProfiles,
                m => m.UserId,
                p => p.UserId,
                (m, profiles) => new EventMessageViewModel
                {
                    Id = m.Id,
                    EventId = m.EventId,
                    UserId = m.UserId,
                    Message = m.Message,
                    CreatedAt = m.CreatedAt,
                    NickName = profiles.Select(p => p.NickName).FirstOrDefault() ?? "匿名使用者",
                    Photo = profiles.Select(p => p.Photo).FirstOrDefault()
                }).OrderBy(m => m.CreatedAt).ToListAsync();
            var participants = await _context.EventParticipants
                .Where(p => p.EventId == id)
                .GroupJoin(
                    _context.PlayerProfiles,
                    p => p.UserId,
                    profile => profile.UserId,
                    (p, profiles) => new EventParticipantViewModel
                    {
                        UserId = p.UserId,
                        NickName = profiles.Select(x => x.NickName).FirstOrDefault() ?? "匿名使用者",
                        Photo = profiles.Select(x => x.Photo).FirstOrDefault(),
                        Age = profiles.Select(x => x.Age).FirstOrDefault(),
                        SkillLevel = profiles.Select(x => x.SkillLevel).FirstOrDefault()
                    }
                )
                .ToListAsync();

            ViewBag.Participants = participants;

            ViewBag.Messages = messages;
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Join(int eventId)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);

            

            if (ev == null) 
            { return NotFound(); }

            if (ev.OrganizerUserId == userId)
            {
                TempData["Message"] = "發起人不能報名自己的活動";
                return RedirectToAction("Details", new { id = eventId });
            }

            bool alreadyJoined = await _context.EventParticipants
                .AnyAsync(p => p.UserId == userId && p.EventId == eventId);
            if (alreadyJoined)
            {
                TempData["Message"] = "你已經報名過此活動";
                return RedirectToAction("Details", new { id = eventId });
            }
            int currentCount = await _context.EventParticipants
                .CountAsync(e => e.EventId == eventId);
            if (currentCount >= ev.MaxPlayers)
            {
                TempData["Message"] = "此活動人數已滿";
                return RedirectToAction("Details", new { id = eventId });
            }
            var participant = new EventParticipant
            {
                EventId = eventId,
                UserId = userId
            };
            _context.EventParticipants.Add(participant);
            await _context.SaveChangesAsync();

            TempData["Message"] = "報名成功";
            return RedirectToAction("Details", new {id=eventId});
        }

        public async Task<IActionResult> ByCourt(int courtId)
        {
            var court = await _context.Courts.FirstOrDefaultAsync(c => c.Id == courtId);
            if (court == null)
            {
                return NotFound();
            }

            var events = await _context.Events
                .Include(e => e.Participants)
                .Where(e => e.CourtId == courtId)
                .OrderBy(e => e.EventDate)
                .ThenBy(e => e.StartTime)
                .ToListAsync();

            ViewBag.CourtId = court.Id;
            ViewBag.CourtName = court.Name;
            return View(events);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CancelJoin(int eventId)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var participant = await _context.EventParticipants
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId);

            if (participant == null)
            {
                TempData["messeage"] = "你尚未報名此活動";
                return RedirectToAction("Details",new {id=eventId});
               
            }
            _context.EventParticipants.Remove(participant);
            await _context.SaveChangesAsync();

            TempData["Message"] = "你已取消報名";
            return RedirectToAction("Details", new {id=eventId});
        }
        [Authorize]
        public async Task<IActionResult> MyEvents()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Forbid();

            var created = await _context.Events
                .Where(e => e.OrganizerUserId == userId)
                .Select(e => new EventItem
                {
                    Id = e.Id,
                    Title = e.Title,
                    CourtName = e.Court != null ? e.Court.Name : "",
                    EventDate = e.EventDate,
                    MaxPlayers = e.MaxPlayers,
                    CurrentCount = e.Participants.Count(),
                    IsFull=e.Participants.Count() >= e.MaxPlayers,
                    IsJoined=true,
                    IsOwner=true
                })
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            var joined=await _context.EventParticipants
                .Where(p => p.UserId==userId)
                .Select(p => new EventItem
                {
                    Id = p.Event!.Id,
                    Title = p.Event.Title,
                    CourtName = p.Event.Court!.Name,
                    EventDate = p.Event.EventDate,
                    MaxPlayers = p.Event.MaxPlayers,
                    CurrentCount = p.Event.Participants.Count(),
                    IsFull = p.Event.Participants.Count() >= p.Event.MaxPlayers,
                    IsOwner = p.Event.OrganizerUserId == userId,
                    IsJoined = true
                })
                .OrderBy(e => e.EventDate) .ToListAsync();

            var vm = new MyEventsViewModel
            {
                CreatedEvents = created,
                JoinedEvents = joined
            };


            return View(vm);

        }

        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Forbid();

            var ev = await _context.Events
                .Include(e => e.Court)
                .FirstOrDefaultAsync(e => e.Id==id);

            if (ev == null)
            {
                return NotFound();

            }
            if (ev.OrganizerUserId != userId)
            {
                return Forbid();
            }
            return View(ev);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Forbid();

            var ev = await _context.Events
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound();

            if (ev.OrganizerUserId != userId)
                return Forbid();
            _context.EventParticipants.RemoveRange(ev.Participants);

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            TempData["Message"] = "活動已刪除";
            return RedirectToAction(nameof(MyEvents));
        }
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var ev = await _context.Events
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
            {
                return NotFound();
            }

            if (ev.OrganizerUserId != userId)
            {
                return Forbid();
            }

            ViewBag.CourtList = new SelectList(
                await _context.Courts.ToListAsync(),
                "Id",
                "Name",
                ev.CourtId
            );

            ViewBag.CurrentParticipantCount = ev.Participants.Count;

            return View(ev);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event ev)
        {
            if (id != ev.Id)
            {
                return NotFound();
            }

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var evDb = await _context.Events
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evDb == null)
            {
                return NotFound();
            }

            // 只有發起人能修改
            if (evDb.OrganizerUserId != userId)
            {
                return Forbid();
            }

            // 因為 OrganizerUserId 不會從表單傳回，所以先移除驗證
            ModelState.Remove(nameof(Event.OrganizerUserId));

            // 驗證：結束時間必須大於開始時間
            if (ev.EndTime <= ev.StartTime)
            {
                ModelState.AddModelError(nameof(Event.EndTime), "結束時間必須晚於開始時間");
            }

            // 驗證：活動開始時間不能早於現在
            DateTime startDateTime = ev.EventDate.Date.Add(ev.StartTime);
            if (startDateTime < DateTime.Now)
            {
                ModelState.AddModelError(nameof(Event.StartTime), "活動開始時間不能早於現在");
            }

            // 驗證：最大人數不能小於目前已報名人數
            int currentParticipantCount = evDb.Participants.Count;
            if (ev.MaxPlayers < currentParticipantCount)
            {
                ModelState.AddModelError(nameof(Event.MaxPlayers), $"最大人數不能小於目前已報名人數（{currentParticipantCount} 人）");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CourtList = new SelectList(
                    await _context.Courts.ToListAsync(),
                    "Id",
                    "Name",
                    ev.CourtId
                );

                ViewBag.CurrentParticipantCount = currentParticipantCount;

                return View(ev);
            }

            // 只更新允許修改的欄位
            evDb.Title = ev.Title;
            evDb.CourtId = ev.CourtId;
            evDb.EventDate = ev.EventDate;
            evDb.StartTime = ev.StartTime;
            evDb.EndTime = ev.EndTime;
            evDb.MaxPlayers = ev.MaxPlayers;
            evDb.SkillLevel = ev.SkillLevel;
            evDb.Description = ev.Description;

            await _context.SaveChangesAsync();

            TempData["Message"] = "活動修改成功";
            return RedirectToAction("Details", new { id = evDb.Id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddMessage(int eventId, string message)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequest("留言不能空白");
            }

            var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev == null)
            {
                return NotFound();
            }

            var msg = new EventMessage
            {
                EventId = eventId,
                UserId = userId,
                Message = message.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.EventMessages.Add(msg);
            await _context.SaveChangesAsync();

            var profile = await _context.PlayerProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var nickName = profile?.NickName ?? "匿名使用者";
            var photo = profile?.Photo ?? "/images/default.png";

            await _hubContext.Clients
                .Group($"Event-{eventId}")
                .SendAsync("ReceiveMessage", new
                {
                    id = msg.Id,
                    eventId = msg.EventId,
                    userId = msg.UserId,
                    nickName = nickName,
                    photo = photo,
                    message = msg.Message,
                    createdAt = msg.CreatedAt.ToString("yyyy/MM/dd HH:mm")
                });

            return Ok();
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var msg = await _context.EventMessages.FirstOrDefaultAsync(m => m.Id == id);

            if (msg == null)
            {
                return NotFound();
            }

            if (msg.UserId != userId)
            {
                return Forbid();
            }

            int eventId = msg.EventId;

            _context.EventMessages.Remove(msg);
            await _context.SaveChangesAsync();

            TempData["Message"] = "留言已刪除";
            return RedirectToAction("Details", new { id = eventId });
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveParticipant(int eventId, string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);

            if (ev == null)
                return NotFound();

            // 只有發起人能踢人
            if (ev.OrganizerUserId != currentUserId)
                return Forbid();

            var participant = await _context.EventParticipants
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId);

            if (participant == null)
                return NotFound();

            _context.EventParticipants.Remove(participant);
            await _context.SaveChangesAsync();

            TempData["Message"] = "已移除參加者";
            return RedirectToAction("Details", new { id = eventId });
        }


    }
    }
