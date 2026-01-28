using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PulseWord.Core.Models;
using PulseWord.Core.Services;
using PulseWord.Infrastructure.Data;
using PulseWord.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PulseWord.Api.Controllers
{
    /// <summary>
    /// Controller for managing users and social interactions.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/users")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly ISocialService _socialService;
        private readonly PulseWordContext _context;

        public UsersController(ISocialService socialService, PulseWordContext context)
        {
            _socialService = socialService;
            _context = context;
        }

        /// <summary>
        /// Gets a user's profile information.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>The user's profile.</returns>
        /// <response code="200">Returns the user profile.</response>
        /// <response code="404">If the user is not found.</response>
        [HttpGet("{id}/profile")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileDto>> GetUserProfile(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.PlayerGames)
                .Include(u => u.ReceivedApplause)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var profile = new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName,
                DisplayName = user.DisplayName ?? user.UserName,
                AvatarUrl = user.AvatarUrl,
                IsAnonymous = user.IsAnonymous,
                TotalGames = user.PlayerGames.Count,
                Wins = user.PlayerGames.Count(pg => pg.Result == GameResult.Win),
                ApplauseCount = user.ReceivedApplause.Count
            };

            return Ok(profile);
        }

        /// <summary>
        /// Follows or unfollows a user.
        /// </summary>
        /// <param name="targetUserId">The ID of the user to follow/unfollow.</param>
        /// <returns>No content on success.</returns>
        /// <response code="204">If the follow action was successful.</response>
        /// <response code="400">If the request is invalid.</response>
        [HttpPost("{targetUserId}/follow")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> FollowUser(Guid targetUserId)
        {
            var followerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(followerIdClaim, out var followerId))
            {
                return Unauthorized();
            }

            await _socialService.ToggleFollowAsync(followerId, targetUserId);
            return NoContent();
        }

        /// <summary>
        /// Gets the followers of a user.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>A list of user profiles.</returns>
        [HttpGet("{id}/followers")]
        [ProducesResponseType(typeof(List<UserProfileDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<UserProfileDto>>> GetFollowers(Guid id)
        {
            var followers = await _socialService.GetFollowersAsync(id);
            return Ok(followers);
        }

        /// <summary>
        /// Gets the users followed by a user.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>A list of user profiles.</returns>
        [HttpGet("{id}/following")]
        [ProducesResponseType(typeof(List<UserProfileDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<UserProfileDto>>> GetFollowing(Guid id)
        {
            var following = await _socialService.GetFollowingAsync(id);
            return Ok(following);
        }
    }
}
