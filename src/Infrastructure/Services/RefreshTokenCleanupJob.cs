using Bookazone.Infrastructure.Persistence;
using Bookazone.Infrastructure.Persistence.DbContext;

namespace Bookazone.Infrastructure.Services;

public class RefreshTokenCleanupJob(IServiceScopeFactory scopeFactory, ILogger<RefreshTokenCleanupJob> logger)
    : IHostedService
{
    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(Cleanup, null, TimeSpan.Zero, TimeSpan.FromHours(24));
        return Task.CompletedTask;
    }

    private async void Cleanup(object? state)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BookazoneDbContext>();

        var threshold = DateTime.UtcNow.AddDays(-7);
        var oldTokens = db.RefreshTokens.Where(r => r.ExpiryDate < threshold || r.Revoked).ToList();

        if (oldTokens.Count > 0)
        {
            db.RefreshTokens.RemoveRange(oldTokens);
            await db.SaveChangesAsync();
            logger.LogInformation("🧹 Cleaned up {Count} old refresh tokens", oldTokens.Count);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        return Task.CompletedTask;
    }
}
