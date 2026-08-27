using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Features.Billing;
using SewaRent_Api.Features.Notification;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Shared.Infrastructure.BackgroundJobs;

public class ScheduledPaymentReminderJob(IServiceScopeFactory scopeFactory, ILogger<ScheduledPaymentReminderJob> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddDays(1).AddHours(1);
            var delay = nextRun - now;

            await Task.Delay(delay, stoppingToken);

            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SewaRentDbContext>();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var today = DateTime.UtcNow.Day;
                var approvedRentalRequests = await db.RentalRequests
                    .Where(r => !r.IsDeleted && r.StatusId == db.RentalRequestStatuses
                        .Where(s => s.Name == "Approved" && !s.IsDeleted)
                        .Select(s => s.Id)
                        .FirstOrDefault())
                    .ToListAsync(stoppingToken);

                foreach (var rentalRequest in approvedRentalRequests)
                {
                    var existingNotification = await db.PaymentNotifications
                        .AnyAsync(n => n.RentalRequestId == rentalRequest.Id
                            && n.NotificationType == "Scheduled"
                            && n.ScheduleDay == today
                            && n.SentAt.Month == now.Month
                            && n.SentAt.Year == now.Year
                            && !n.IsDeleted, stoppingToken);

                    if (existingNotification)
                        continue;

                    var invoiceCommand = new GenerateInvoice.Command(
                        rentalRequest.Id,
                        now.Month,
                        now.Year);

                    var invoiceResult = await mediator.Send(invoiceCommand, stoppingToken);

                    var notificationCommand = new SendScheduledPaymentNotification.Command(
                        rentalRequest.Id,
                        today,
                        invoiceResult.Id,
                        $"Your invoice for {now:MMMM yyyy} has been generated. Amount: {invoiceResult.TotalAmount:C2}. Due: {invoiceResult.DueDate:dd MMM yyyy}.");

                    await mediator.Send(notificationCommand, stoppingToken);
                }

                logger.LogInformation("Scheduled payment reminder job completed at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error running scheduled payment reminder job.");
            }
        }
    }
}
