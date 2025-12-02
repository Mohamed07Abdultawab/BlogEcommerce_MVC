using BlogEcommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogEcommerce.Services
{
    public class OrderStatusService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderStatusService> _logger;

        public OrderStatusService(IServiceProvider serviceProvider, ILogger<OrderStatusService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        await UpdateOrderStatuses(context);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred updating order statuses.");
                }

                // Run every hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task UpdateOrderStatuses(ApplicationDbContext context)
        {
            var now = DateTime.Now;

            // Auto-update Pending to Processing after 1 hour
            var pendingOrders = await context.Orders
                .Where(o => o.Status == "Pending" && o.OrderDate.AddHours(1) <= now)
                .ToListAsync();

            foreach (var order in pendingOrders)
            {
                order.Status = "Processing";
                context.Update(order);
            }

            // Auto-update Processing to Shipped after 2 days
            var processingOrders = await context.Orders
                .Where(o => o.Status == "Processing" && o.OrderDate.AddDays(2) <= now)
                .ToListAsync();

            foreach (var order in processingOrders)
            {
                order.Status = "Shipped";
                context.Update(order);
            }

            // Auto-update Shipped to Completed after 5 days
            var shippedOrders = await context.Orders
                .Where(o => o.Status == "Shipped" && o.OrderDate.AddDays(5) <= now)
                .ToListAsync();

            foreach (var order in shippedOrders)
            {
                order.Status = "Completed";
                context.Update(order);
            }

            if (pendingOrders.Any() || processingOrders.Any() || shippedOrders.Any())
            {
                await context.SaveChangesAsync();
                _logger.LogInformation($"Updated {pendingOrders.Count + processingOrders.Count + shippedOrders.Count} order statuses.");
            }
        }
    }
}