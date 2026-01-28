using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PulseWord.Api.Hubs;
using PulseWord.Api.Models;
using PulseWord.Core.Entities;
using PulseWord.Core.Services;
using PulseWord.Infrastructure.Data;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PulseWord.Api.Controllers
{
    /// <summary>
    /// Controller for managing game sessions and actions.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/games")]
    [Produces("application/json")]
    public class GamesController : ControllerBase
    {
        private readonly ISocialService _socialService;
        private readonly IHubContext<PulseHub> _hubContext;
        private readonly PulseWordContext _context;
        private readonly IWordleService _wordleService;

        public GamesController(ISocialService socialService, IHubContext<PulseHub> hubContext, PulseWordContext context, IWordleService wordleService)
        {
            _socialService = socialService;
            _hubContext = hubContext;
            _context = context;
            _wordleService = wordleService;
        }
        /// <summary>
        /// Joins a daily game.
        /// </summary>
        /// <param name="dailyGameId">The ID of the daily game to join.</param>
        /// <returns>The player's game state.</returns>
        /// <response code="201">Returns the newly created player game state.</response>
        /// <response code="400">If the game cannot be joined.</response>
        [HttpPost("{dailyGameId}/join")]
        [ProducesResponseType(typeof(PlayerGameDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PlayerGameDto>> JoinGame(Guid dailyGameId)
        {
            // Placeholder logic
            var playerGame = new PlayerGameDto
            {
                Id = Guid.NewGuid(),
                StartTime = DateTime.UtcNow,
                GuessesCount = 0,
                Result = "InProgress",
                RemainingTime = TimeSpan.FromMinutes(10)
            };

            return await Task.FromResult(CreatedAtAction(nameof(GetGameStatus), new { dailyGameId = dailyGameId }, playerGame));
        }

        /// <summary>
        /// Submits a guess for a daily game.
        /// </summary>
        /// <param name="dailyGameId">The ID of the daily game.</param>
        /// <param name="request">The guess request containing the word.</param>
        /// <returns>The result of the guess.</returns>
        /// <response code="200">Returns the feedback for the guess.</response>
        /// <response code="400">If the guess is invalid.</response>
        [HttpPost("{dailyGameId}/guesses")]
        [ProducesResponseType(typeof(GuessResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GuessResponseDto>> SubmitGuess(Guid dailyGameId, [FromBody] GuessRequestDto request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var guessWord = request.GuessWord?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(guessWord) || guessWord.Length != 5)
            {
                return BadRequest("Guess must be 5 letters long.");
            }

            if (!_wordleService.IsValidWord(guessWord))
            {
                return BadRequest("Invalid word.");
            }

            var dailyGame = await _context.DailyGames.FindAsync(dailyGameId);
            if (dailyGame == null)
            {
                return NotFound("Daily game not found.");
            }

            var playerGame = await _context.PlayerGames
                .Include(pg => pg.Guesses)
                .FirstOrDefaultAsync(pg => pg.DailyGameId == dailyGameId && pg.UserId == userId);

            if (playerGame == null)
            {
                return BadRequest("Player has not joined this game.");
            }

            if (playerGame.Completed)
            {
                return BadRequest("Game already completed.");
            }

            var coreFeedback = _wordleService.CalculateFeedback(guessWord, dailyGame.SeedWord);
            
            // Map core feedback to API feedback
            var apiFeedback = coreFeedback.Select(f => f switch
            {
                PulseWord.Core.Entities.LetterFeedback.CorrectPosition => PulseWord.Api.Models.LetterFeedback.Correct,
                PulseWord.Core.Entities.LetterFeedback.CorrectLetterWrongPosition => PulseWord.Api.Models.LetterFeedback.Misplaced,
                _ => PulseWord.Api.Models.LetterFeedback.Incorrect
            }).ToArray();

            var guess = new Guess
            {
                Id = Guid.NewGuid(),
                PlayerGameId = playerGame.Id,
                SubmittedAt = DateTimeOffset.UtcNow,
                GuessWord = guessWord,
                Feedback = JsonSerializer.Serialize(coreFeedback)
            };

            _context.Guesses.Add(guess);
            playerGame.GuessesCount++;

            bool won = coreFeedback.All(f => f == PulseWord.Core.Entities.LetterFeedback.CorrectPosition);
            bool lost = !won && playerGame.GuessesCount >= 6;

            if (won || lost)
            {
                playerGame.Completed = true;
                playerGame.EndTime = DateTimeOffset.UtcNow;
                playerGame.CompletionTime = playerGame.EndTime - playerGame.StartTime;
                playerGame.Result = won ? GameResult.Win : GameResult.Fail;
            }

            await _context.SaveChangesAsync();

            var response = new GuessResponseDto
            {
                GuessWord = guessWord,
                Feedback = apiFeedback,
                Result = won ? "Win" : (lost ? "Fail" : "Incorrect"),
                RemainingTime = dailyGame.TimeWindowEnd - DateTimeOffset.UtcNow,
                IsComplete = playerGame.Completed
            };

            return Ok(response);
        }

        /// <summary>
        /// Gets the current status of a player's game.
        /// </summary>
        /// <param name="dailyGameId">The ID of the daily game.</param>
        /// <returns>The current player game state.</returns>
        /// <response code="200">Returns the player's game status.</response>
        /// <response code="404">If the game session is not found.</response>
        [HttpGet("{dailyGameId}/status")]
        [ProducesResponseType(typeof(PlayerGameDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PlayerGameDto>> GetGameStatus(Guid dailyGameId)
        {
            // Placeholder logic
            var playerGame = new PlayerGameDto
            {
                Id = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddMinutes(-1),
                GuessesCount = 1,
                Result = "InProgress",
                RemainingTime = TimeSpan.FromMinutes(9)
            };

            return await Task.FromResult(Ok(playerGame));
        }

        /// <summary>
        /// Applauds another player in a daily game.
        /// </summary>
        /// <param name="dailyGameId">The ID of the daily game.</param>
        /// <param name="request">The request containing the target user ID.</param>
        /// <returns>No content on success.</returns>
        /// <response code="204">If the applause was successfully recorded.</response>
        /// <response code="400">If the request is invalid.</response>
        [HttpPost("{dailyGameId}/applaud")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApplaudPlayer(Guid dailyGameId, [FromBody] ApplauseRequestDto request)
        {
            var fromUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(fromUserIdClaim, out var fromUserId))
            {
                return Unauthorized();
            }

            var success = await _socialService.ApplaudAsync(fromUserId, request.ToUserId, dailyGameId);
            if (!success)
            {
                return BadRequest("Could not record applause. You may have already applauded this player for this game, or you are trying to applaud yourself.");
            }

            // Get names for broadcast
            var fromUser = await _context.Users.FindAsync(fromUserId);
            var toUser = await _context.Users.FindAsync(request.ToUserId);
            var game = await _context.DailyGames.FindAsync(dailyGameId);

            if (fromUser != null && toUser != null && game != null)
            {
                await _hubContext.Clients.Group(game.Date.ToString("yyyy-MM-dd"))
                    .SendAsync("ApplauseReceived", new { FromUser = fromUser.DisplayName ?? fromUser.UserName, ToUser = toUser.DisplayName ?? toUser.UserName });
            }

            return NoContent();
        }
    }
}
