namespace Ruway.Events.Command.Interfaces.Events;

/// <summary>
/// Interface para el publicador de eventos
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publica un evento de dominio
    /// </summary>
    /// <param name="domainEvent">Evento a publicar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Publica múltiples eventos de dominio
    /// </summary>
    /// <param name="domainEvents">Eventos a publicar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}