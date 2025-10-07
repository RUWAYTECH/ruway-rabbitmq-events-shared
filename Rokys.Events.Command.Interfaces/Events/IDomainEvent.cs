using MediatR;

namespace Rokys.Events.Command.Interfaces.Events;

/// <summary>
/// Interface base para todos los eventos del dominio
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Identificador único del evento
    /// </summary>
    Guid EventId { get; }
    
    /// <summary>
    /// Timestamp de cuando ocurrió el evento
    /// </summary>
    DateTime OccurredOn { get; }
    
    /// <summary>
    /// Nombre del evento
    /// </summary>
    string EventName { get; }
    
    /// <summary>
    /// Versión del evento para versionado
    /// </summary>
    int Version { get; }
}