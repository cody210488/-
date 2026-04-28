using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 打球啊.Data;
using 打球啊.Models;

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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourt(int id)
        {
            var courts = await _context.Courts.Where(c => c.Id == id).Select(
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
        [HttpPost]
        public async Task<IActionResult> CreateCourt([FromBody] Court court)
        {
            if (ModelState.IsValid)
            {
                _context.Courts.Add(court);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetCourt), new { id = court.Id }, court);
            }
            return BadRequest(ModelState);

        }
    }
}