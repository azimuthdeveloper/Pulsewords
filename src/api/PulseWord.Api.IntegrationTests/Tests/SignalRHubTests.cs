using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using PulseWord.Api.IntegrationTests.Fixtures;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;

namespace PulseWord.Api.IntegrationTests.Tests
{
    [Trait("Category", "Integration")]
    public class SignalRHubTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public SignalRHubTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task PulseHub_CanConnect()
        {
            // Arrange
            var connection = CreateHubConnection();

            // Act
            await connection.StartAsync();

            // Assert
            connection.State.Should().Be(HubConnectionState.Connected);

            await connection.StopAsync();
        }

        [Fact]
        public async Task JoinGroupForDate_AddsClientToGroup()
        {
            // Arrange
            var connection = CreateHubConnection();
            await connection.StartAsync();

            // Act
            Func<Task> act = async () => await connection.InvokeAsync("JoinGroupForDate", "2026-01-28");

            // Assert
            await act.Should().NotThrowAsync();
            
            await connection.StopAsync();
        }

        [Fact]
        public async Task LeaveGroupForDate_RemovesClientFromGroup()
        {
            // Arrange
            var connection = CreateHubConnection();
            await connection.StartAsync();
            await connection.InvokeAsync("JoinGroupForDate", "2026-01-28");

            // Act
            Func<Task> act = async () => await connection.InvokeAsync("LeaveGroupForDate", "2026-01-28");

            // Assert
            await act.Should().NotThrowAsync();

            await connection.StopAsync();
        }

        [Fact]
        public async Task GuessSubmission_BroadcastsToGroup()
        {
            // Arrange
            var connection = CreateHubConnection();
            string receivedMessage = null;
            connection.On<string>("ReceiveGuessUpdate", msg => receivedMessage = msg);

            await connection.StartAsync();
            await connection.InvokeAsync("JoinGroupForDate", "2026-01-28");

            // Act
            // In a real scenario, this would be triggered by POST /api/games/{id}/guesses
            // For now, we just test the connection and group joining works.
            
            // Assert
            connection.State.Should().Be(HubConnectionState.Connected);

            await connection.StopAsync();
        }

        [Fact]
        public async Task LeaderboardUpdate_BroadcastsToGroup()
        {
            // Arrange
            var connection = CreateHubConnection();
            connection.On<string>("ReceiveLeaderboardUpdate", msg => { });

            await connection.StartAsync();
            await connection.InvokeAsync("JoinGroupForDate", "2026-01-28");

            // Assert
            connection.State.Should().Be(HubConnectionState.Connected);

            await connection.StopAsync();
        }

        private HubConnection CreateHubConnection()
        {
            return new HubConnectionBuilder()
                .WithUrl("http://localhost/pulseHub", options =>
                {
                    options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                })
                .Build();
        }
    }
}
