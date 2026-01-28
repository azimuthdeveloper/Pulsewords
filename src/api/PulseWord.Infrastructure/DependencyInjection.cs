using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PulseWord.Core.Services;
using PulseWord.Infrastructure.Data;
using PulseWord.Infrastructure.Services;

namespace PulseWord.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PulseWord") 
            ?? throw new InvalidOperationException("Connection string 'PulseWord' not found.");

        services.AddDbContext<PulseWordContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IWordleService, WordleService>();
        services.AddScoped<ILeaderboardService, LeaderboardService>();
        services.AddScoped<ISocialService, SocialService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
