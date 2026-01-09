using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ruway.Events.Command.Configuration;
using Ruway.Events.Command.Interfaces.Events;

namespace Ruway.Events.Command.Events;

/// <summary>
/// Implementación del publicador de eventos usando MediatR y RabbitMQ
/// </summary>
public class EventPublisher : IEventPublisher
{
    private readonly IMediator _mediator;
    private readonly IRabbitMQService _rabbitMQService;
    private readonly RabbitMQSettings _rabbitMQSettings;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(
        IMediator mediator,
        IRabbitMQService rabbitMQService,
        IOptions<RabbitMQSettings> rabbitMQSettings,
        ILogger<EventPublisher> logger)
    {
        _mediator = mediator;
        _rabbitMQService = rabbitMQService;
        _rabbitMQSettings = rabbitMQSettings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Publishing domain event: {EventName} with ID: {EventId}", 
                domainEvent.EventName, domainEvent.EventId);

            // 1. Publicar localmente usando MediatR para handlers internos
            await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogDebug("Domain event published locally via MediatR: {EventName}", domainEvent.EventName);

            // 2. Publicar externamente usando RabbitMQ para integración
            var routingKey = GetRoutingKeyForEvent(domainEvent);
            await _rabbitMQService.PublishEventAsync(domainEvent, routingKey, cancellationToken);
            _logger.LogDebug("Domain event published to RabbitMQ: {EventName} with routing key: {RoutingKey}", 
                domainEvent.EventName, routingKey);

            _logger.LogInformation("Successfully published domain event: {EventName} with ID: {EventId} to both MediatR and RabbitMQ", 
                domainEvent.EventName, domainEvent.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing domain event: {EventName} with ID: {EventId}", 
                domainEvent.EventName, domainEvent.EventId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        var events = domainEvents.ToList();
        
        _logger.LogInformation("Publishing {Count} domain events", events.Count);

        foreach (var domainEvent in events)
        {
            await PublishAsync(domainEvent, cancellationToken);
        }
    }

    /// <summary>
    /// Construye la routing key basada en las propiedades del evento o configuración
    /// </summary>
    /// <param name="domainEvent">Evento de dominio</param>
    /// <returns>Routing key para RabbitMQ</returns>
    private string GetRoutingKeyForEvent(IDomainEvent domainEvent)
    {
        // 1. Si el evento implementa IRoutableEvent, usar su routing key personalizada
        if (domainEvent is IRoutableEvent routableEvent && !string.IsNullOrEmpty(routableEvent.RoutingKey))
        {
            return routableEvent.RoutingKey;
        }

        // 2. Si existe configuración específica para esta entidad, usarla
        var (entity, action) = ExtractEntityAndActionFromEventName(domainEvent.EventName);
        

        // 4. Usar patrón genérico: microservicio.entidad.acción
        var microservice = _rabbitMQSettings.MicroserviceName?.ToLower() ?? "unknown";
        return $"{microservice}.{entity}.{action}";
    }

    /// <summary>
    /// Extrae la entidad y acción del nombre del evento
    /// </summary>
    /// <param name="eventName">Nombre del evento</param>
    /// <returns>Tupla con (entidad, acción)</returns>
    private (string entity, string action) ExtractEntityAndActionFromEventName(string eventName)
    {
        var lowerEventName = eventName.ToLowerInvariant();
        
        // Remover el sufijo "event" si existe
        if (lowerEventName.EndsWith("event"))
        {
            lowerEventName = lowerEventName[..^5]; // Remove "event"
        }

        // Definir acciones reconocidas
        var actions = new[] { "created", "updated", "deleted", "activated", "deactivated", "assigned", "unassigned", "approved", "rejected", "sent", "received" };
        
        // Buscar acción al final del nombre
        string action = "unknown";
        string entity = lowerEventName;

        foreach (var act in actions)
        {
            if (lowerEventName.EndsWith(act))
            {
                action = act;
                entity = lowerEventName[..^act.Length]; // Remove action
                break;
            }
        }

        // Limpiar entidad (remover caracteres no válidos, convertir a snake_case si es necesario)
        entity = CleanEntityName(entity);

        return (entity, action);
    }

    /// <summary>
    /// Limpia el nombre de la entidad para usar en routing keys
    /// </summary>
    /// <param name="entity">Nombre de la entidad</param>
    /// <returns>Nombre limpio de la entidad</returns>
    private string CleanEntityName(string entity)
    {
        if (string.IsNullOrEmpty(entity))
            return "unknown";

        // Convertir de PascalCase a snake_case para routing keys
        var result = System.Text.RegularExpressions.Regex.Replace(entity, 
            "([a-z0-9])([A-Z])", "$1_$2").ToLower();
            
        return result;
    }
}