using Ruway.Events.Command.Interfaces.Events;

namespace Ruway.Events.Command.Events;

/// <summary>
/// Clase base para eventos de dominio
/// </summary>
public abstract class DomainEventBase : IDomainEvent
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="eventName">Nombre del evento</param>
    /// <param name="version">Versión del evento</param>
    protected DomainEventBase(string eventName, int version = 1)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        EventName = eventName;
        Version = version;
    }

    /// <inheritdoc />
    public Guid EventId { get; }

    /// <inheritdoc />
    public DateTime OccurredOn { get; }

    /// <inheritdoc />
    public string EventName { get; }

    /// <inheritdoc />
    public int Version { get; }
}