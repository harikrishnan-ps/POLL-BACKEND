using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using poll_api.Application.DTOs.Poll;
using poll_api.Application.Interfaces;
using System.Security.Claims;

namespace poll_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PollsController : ControllerBase
    {
        private readonly IPollService _pollService;

        public PollsController(IPollService pollService)
        {
            _pollService = pollService;
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdClaim != null ? int.Parse(userIdClaim) : null;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllPolls()
        {
            var userId = GetUserId();
            var isAdmin = IsAdmin();
            var polls = await _pollService.GetAllPollsAsync(userId, isAdmin);
            return Ok(polls);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPollById(int id)
        {
            var userId = GetUserId();
            var isAdmin = IsAdmin();
            var poll = await _pollService.GetPollByIdAsync(id, userId, isAdmin);
            return Ok(poll);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePoll([FromBody] CreatePollDto createPollDto)
        {
            var poll = await _pollService.CreatePollAsync(createPollDto);
            return CreatedAtAction(nameof(GetPollById), new { id = poll.Id }, poll);
        }

        [HttpPost("{id}/reveal")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RevealResults(int id)
        {
            await _pollService.RevealResultsAsync(id);
            return Ok(new { message = "Results revealed successfully." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePoll(int id)
        {
            await _pollService.DeletePollAsync(id);
            return Ok(new { message = "Poll deleted successfully." });
        }

        [HttpPost("vote")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> SubmitVote([FromBody] VoteDto voteDto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            await _pollService.SubmitVoteAsync(voteDto, userId.Value);
            return Ok(new { message = "Vote submitted successfully." });
        }
    }
}
