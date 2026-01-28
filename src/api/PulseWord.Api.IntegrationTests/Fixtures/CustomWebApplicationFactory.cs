using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PulseWord.Infrastructure.Data;
using PulseWord.Core.Entities;

namespace PulseWord.Api.IntegrationTests.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;

    new public Task DisposeAsync() => Task.CompletedTask;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove the app's PulseWordContext registration.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<PulseWordContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add PulseWordContext using an in-memory database for testing.
            services.AddDbContext<PulseWordContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            // Replace real auth with TestAuthHandler
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
            })
            .AddScheme<TestAuthHandlerOptions, TestAuthHandler>(
                TestAuthHandler.AuthenticationScheme, options => { });

            // Seed test data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<PulseWordContext>();
            var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

            db.Database.EnsureCreated();

            try
            {
                SeedData(db);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the " +
                    "database with test messages. Error: {Message}", ex.Message);
            }
        });
    }

    private static readonly Guid TestUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private void SeedData(PulseWordContext db)
    {
        if (!db.Users.Any())
        {
            db.Users.Add(new User 
            { 
                Id = TestUserId, 
                UserName = "test-user", 
                DisplayName = "Test User",
                CreatedAt = DateTimeOffset.UtcNow,
                Preferences = "{}"
            });
            db.SaveChanges();
        }
    }
}
