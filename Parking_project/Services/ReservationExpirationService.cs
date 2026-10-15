using Microsoft.EntityFrameworkCore;
using Parking_project.Data;

namespace Parking_project.Services
{

    public class ReservationExpirationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReservationExpirationService> _logger;

        public ReservationExpirationService(IServiceScopeFactory scopeFactory, ILogger<ReservationExpirationService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CancelExpiredReservationsAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while cancelling expired reservations.");
                }

                try
                {
                    if (!await timer.WaitForNextTickAsync(stoppingToken))
                        break;
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private async Task CancelExpiredReservationsAsync(
            CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AplicationDbContext>();

            var expirationTime = DateTime.UtcNow.AddHours(-1);

            var expiredReservations = await db.TransaksionParkimi
                .Where(t => t.Statusi == "Reserved" && t.KohaHyrjes <= expirationTime)
                .Include(n=>  n.Njesia)
                .ToListAsync(cancellationToken);

            if (expiredReservations.Count == 0)
                return;

            foreach (var transaction in expiredReservations)
            {
                transaction.Statusi = "Cancelled";
                transaction.Njesia.VendeTeLira++;
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
