using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

/// Класс с фоновой задачей по запуску удаления не подтвержденных email

namespace SUPPLY_API 
{
    public class EmailCleanupHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailCleanupHostedService> _logger;

        public EmailCleanupHostedService(IServiceProvider serviceProvider, ILogger<EmailCleanupHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email cleanup hosted service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<CollaboratorSystemContext>();

                        var thresholdTime = DateTime.UtcNow.AddHours(-48);
                        var usersToRemove = await dbContext.CollaboratorSystem
                            .Where(u => !u.ActivationEmailCollaborator && u.DataRegistrationCollaborator < thresholdTime)
                            .ToListAsync(stoppingToken);

                        if (usersToRemove.Any())
                        {
                            dbContext.CollaboratorSystem.RemoveRange(usersToRemove);
                            await dbContext.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation($"Removed {usersToRemove.Count} unconfirmed users.");
                        }
                        else
                        {
                            _logger.LogInformation("No unconfirmed users to remove.");
                        }
                    }

                    // Ожидание до следующей полуночи
                    var currentTime = DateTime.UtcNow;
                    var nextRunTime = currentTime.Date.AddDays(1);
                    var delay = nextRunTime - currentTime;
                    await Task.Delay(delay, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during email cleanup.");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Перезапуск через час в случае ошибки
                }
            }
        }
    }
}