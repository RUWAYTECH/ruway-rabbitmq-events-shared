using Ruway.Events.Command.Interfaces.Events;

namespace Ruway.Events.Command.Events;

/// <summary>
/// Interface para el servicio de RabbitMQ
/// </summary>
public interface IRabbitMQService
{
    /// <summary>
    /// Publica un evento de dominio a RabbitMQ
    /// </summary>
    /// <param name="domainEvent">Evento de dominio a publicar</param>
    /// <param name="routingKey">Routing key para el evento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Task</returns>
    Task PublishEventAsync(IDomainEvent domainEvent, string routingKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si la conexión con RabbitMQ está disponible
    /// </summary>
    /// <returns>True si la conexión está disponible</returns>
    Task<bool> IsHealthyAsync();
}