using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PulseWord.Api.Models;
using PulseWord.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PulseWord.Api.Controllers
{
    /// <summary>
    /// Controller for managing and retrieving daily games.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/dailies")]
    [Produces("application/json")]
    public class DailyGamesController : ControllerBase
    {
        /// <summary>
        /// Gets the daily game info for a specific date.
        /// </summary>
        /// <param name="date">The date in yyyy-MM-dd format.</param>
        /// <returns>The daily game information.</returns>
        /// <response code="200">Returns the daily game info.</response>
        /// <response code="404">If no game is found for the given date.</response>
        [HttpGet("{date}")]
        [ProducesResponseType(typeof(DailyGameDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DailyGameDto>> GetDailyGame(string date)
        {
            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
            {
                return BadRequest("Invalid date format. Use yyyy-MM-dd.");
            }

            // Placeholder logic
            var dailyGame = new DailyGameDto
            {
                Id = Guid.NewGuid(),
                Date = parsedDate,
                TimeWindowStart = parsedDate.AddHours(9),
                TimeWindowEnd = parsedDate.AddHours(10),
                Status = "Active"
            };

            return await Task.FromResult(Ok(dailyGame));
        }

        /// <summary>
        /// Gets the leaderboard for a specific daily game date.
        /// </summary>
        /// <param name="date">The date in yyyy-MM-dd format.</param>
        /// <returns>A list of leaderboard entries.</returns>
        /// <response code="200">Returns the leaderboard.</response>
        [HttpGet("{date}/leaderboard")]
        [ProducesResponseType(typeof(IEnumerable<LeaderboardEntryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LeaderboardEntryDto>>> GetLeaderboard(string date)
        {
            // Placeholder logic
            var leaderboard = new List<LeaderboardEntryDto>
            {
                new LeaderboardEntryDto
                {
                    Rank = 1,
                    UserName = "player1",
                    DisplayName = "Player One",
                    Score = 100,
                    Guesses = 3,
                    CompletedAt = DateTime.UtcNow
                }
            };

            return await Task.FromResult(Ok(leaderboard));
        }
    }
}
