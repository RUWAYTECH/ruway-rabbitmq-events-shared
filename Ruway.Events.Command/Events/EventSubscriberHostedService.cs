using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ruway.Events.Command.Interfaces.Events;

namespace Ruway.Events.Command.Events;

/// <summary>
/// Servicio host para gestionar la suscripción a eventos
/// </summary>
public class EventSubscriberHostedService : BackgroundService
{
    private readonly IEventSubscriber _eventSubscriber;
    private readonly ILogger<EventSubscriberHostedService> _logger;

    public EventSubscriberHostedService(
        IEventSubscriber eventSubscriber,
        ILogger<EventSubscriberHostedService> logger)
    {
        _eventSubscriber = eventSubscriber;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting Event Subscriber Hosted Service");
            
            await _eventSubscriber.StartListeningAsync(stoppingToken);
            
            // Mantener el servicio ejecutándose hasta que se cancele
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Event Subscriber Hosted Service was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Event Subscriber Hosted Service encountered an error");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Event Subscriber Hosted Service");
        
        try
        {
            await _eventSubscriber.StopListeningAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping event subscriber");
        }
        
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _eventSubscriber?.Dispose();
        base.Dispose();
    }
}