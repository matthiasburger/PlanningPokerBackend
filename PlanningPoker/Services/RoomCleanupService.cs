namespace PlanningPoker.Services;

public class RoomCleanupService(IConfiguration cfg, ILogger<RoomCleanupService> logger, RoomManager rooms)
    : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(cfg.GetValue("RoomCleanup:IntervalMinutes", 30));
    private readonly TimeSpan _maxAge = TimeSpan.FromHours(cfg.GetValue("RoomCleanup:MaxRoomAgeHours", 24));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                rooms.Cleanup(_maxAge, TimeSpan.FromMinutes(20));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Room cleanup failed");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}