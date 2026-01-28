using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using PulseWord.Api.Models;
using PulseWord.Core.DTOs;
using PulseWord.Api.IntegrationTests.Fixtures;
using Xunit;

namespace PulseWord.Api.IntegrationTests.Tests
{
    [Trait("Category", "Integration")]
    public class DailyGamesControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public DailyGamesControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetDailyGame_ReturnsDailyGameInfo()
        {
            // Arrange
            var date = DateTime.UtcNow.ToString("yyyy-MM-dd");

            // Act
            var response = await _client.GetAsync($"/api/dailies/{date}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var game = await response.Content.ReadFromJsonAsync<DailyGameDto>();
            game.Should().NotBeNull();
            game.Date.Date.Should().Be(DateTime.Parse(date).Date);
        }

        [Fact]
        public async Task GetDailyGame_Returns404ForNonExistentDate()
        {
            // Arrange
            var date = "2000-01-01";

            // Act
            var response = await _client.GetAsync($"/api/dailies/{date}");

            // Assert
            // Note: Currently returns 200 due to placeholder logic, but test defines expected behavior
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetLeaderboard_ReturnsLeaderboardEntries()
        {
            // Arrange
            var date = DateTime.UtcNow.ToString("yyyy-MM-dd");

            // Act
            var response = await _client.GetAsync($"/api/dailies/{date}/leaderboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var entries = await response.Content.ReadFromJsonAsync<IEnumerable<LeaderboardEntryDto>>();
            entries.Should().NotBeNull();
        }

        [Fact]
        public async Task GetLeaderboard_EmptyForNewGame()
        {
            // Arrange
            var date = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");

            // Act
            var response = await _client.GetAsync($"/api/dailies/{date}/leaderboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var entries = await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>();
            // Note: Currently returns 1 entry due to placeholder logic
            entries.Should().BeEmpty();
        }

        [Fact]
        public async Task GetLeaderboard_OrderingByTimeThenGuesses()
        {
            // Arrange
            var date = DateTime.UtcNow.ToString("yyyy-MM-dd");

            // Act
            var response = await _client.GetAsync($"/api/dailies/{date}/leaderboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var entries = await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>();
            entries.Should().BeInAscendingOrder(e => e.Rank);
        }
    }
}
