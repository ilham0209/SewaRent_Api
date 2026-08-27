using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Features.Notification;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Shared.Infrastructure.BackgroundJobs;

public class OverdueInvoiceCheckJob(IServiceScopeFactory scopeFactory, ILogger<OverdueInvoiceCheckJob> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddDays(1).AddHours(2);
            var delay = nextRun - now;

            await Task.Delay(delay, stoppingToken);

            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SewaRentDbContext>();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var today = DateTime.UtcNow.Date;
                var overdueInvoices = await db.Invoices
                    .Where(i => !i.IsDeleted
                        && (i.Status == "Unpaid" || i.Status == "PaymentClaimed")
                        && i.DueDate < today)
                    .ToListAsync(stoppingToken);

                foreach (var invoice in overdueInvoices)
                {
                    var alreadyNotified = await db.PaymentNotifications
                        .AnyAsync(n => n.InvoiceId == invoice.Id
                            && n.NotificationType == "Overdue"
                            && n.SentAt.Date == today
                            && !n.IsDeleted, stoppingToken);

                    if (alreadyNotified)
                        continue;

                    var command = new SendOverduePaymentNotification.Command(invoice.Id);
                    await mediator.Send(command, stoppingToken);
                }

                logger.LogInformation("Overdue invoice check job completed at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error running overdue invoice check job.");
            }
        }
    }
}
