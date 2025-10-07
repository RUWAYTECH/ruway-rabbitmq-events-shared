using MediatR;

namespace Rokys.Events.Command.Interfaces.Events;

/// <summary>
/// Interface para manejadores de eventos de dominio
/// </summary>
/// <typeparam name="TEvent">Tipo del evento</typeparam>
public interface IDomainEventHandler<in TEvent> : INotificationHandler<TEvent>
    where TEvent : IDomainEvent
{
}