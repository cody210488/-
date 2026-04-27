using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 打球啊.Data;

namespace 打球啊.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourtApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CourtApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetCourt()
        {
            var courts = await _context.Courts.Select(
                c => new
                {
                    c.Id,
                    c.Name,
                    c.Address,
                    c.City,
                    c.District,
                    c.CourtType,
                    c.HasLighting
                }).ToListAsync();
            return Ok(courts);
        }
    }
}
