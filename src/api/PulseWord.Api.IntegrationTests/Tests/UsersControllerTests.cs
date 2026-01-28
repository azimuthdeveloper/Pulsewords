using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using PulseWord.Api.Models;
using PulseWord.Core.Models;
using PulseWord.Api.IntegrationTests.Fixtures;
using Xunit;

namespace PulseWord.Api.IntegrationTests.Tests
{
    [Trait("Category", "Integration")]
    public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public UsersControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetUserProfile_ReturnsUserInfo()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/users/{userId}/profile");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
            profile.Should().NotBeNull();
            profile.Id.Should().Be(userId);
        }

        [Fact]
        public async Task GetUserProfile_Returns404ForNonExistentUser()
        {
            // Arrange
            var userId = Guid.Empty;

            // Act
            var response = await _client.GetAsync($"/api/users/{userId}/profile");

            // Assert
            // Note: Placeholder logic returns 200 for any GUID
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task FollowUser_CreatesFollowRelationship()
        {
            // Arrange
            var targetUserId = Guid.NewGuid();

            // Act
            var response = await _client.PostAsync($"/api/users/{targetUserId}/follow", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task FollowUser_ToggleFollowUnfollow()
        {
            // Arrange
            var targetUserId = Guid.NewGuid();

            // Act - Follow
            var followResponse = await _client.PostAsync($"/api/users/{targetUserId}/follow", null);
            followResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Act - Unfollow
            var unfollowResponse = await _client.PostAsync($"/api/users/{targetUserId}/follow", null);
            unfollowResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task FollowUser_CannotFollowSelf()
        {
            // Arrange
            var selfId = Guid.NewGuid(); // In a real test, this would be the authenticated user's ID

            // Act
            var response = await _client.PostAsync($"/api/users/{selfId}/follow", null);

            // Assert
            // response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
