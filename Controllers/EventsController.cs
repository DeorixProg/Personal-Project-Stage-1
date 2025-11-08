using EventManagement.Application.DTOs.Events;
using EventManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Event_Management.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllPublic()
        {
            var events = await _eventService.GetAllPublicAsync();
            return Ok(events);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var e = await _eventService.GetByIdAsync(id);
            if (e == null) return NotFound();
            return Ok(e);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var created = await _eventService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEventDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            dto.Id = id;
            var success = await _eventService.UpdateAsync(dto, userId);
            if (!success) return Forbid();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var success = await _eventService.DeleteAsync(id, userId);
            if (!success) return Forbid();

            return NoContent();
        }

        [HttpPost("{id}/join")]
        [Authorize]
        public async Task<IActionResult> Join(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var success = await _eventService.JoinEventAsync(id, userId);
            if (!success) return BadRequest("Could not join event (full or already joined).");

            return Ok("Joined successfully.");
        }

        [HttpPost("{id}/leave")]
        [Authorize]
        public async Task<IActionResult> Leave(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var success = await _eventService.LeaveEventAsync(id, userId);
            if (!success) return BadRequest("You are not part of this event.");

            return Ok("Left event successfully.");
        }
    }
}
