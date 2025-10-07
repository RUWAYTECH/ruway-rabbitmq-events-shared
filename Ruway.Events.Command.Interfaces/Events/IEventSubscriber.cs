using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ruway.Events.Command.Interfaces.Events;

/// <summary>
/// Interface para suscribirse a eventos de RabbitMQ
/// </summary>
public interface IEventSubscriber : IDisposable
{
    /// <summary>
    /// Suscribirse a un tipo específico de evento
    /// </summary>
    /// <typeparam name="TEvent">Tipo del evento</typeparam>
    /// <param name="handler">Manejador del evento</param>
    /// <param name="routingKey">Routing key para filtrar eventos</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task SubscribeAsync<TEvent>(
        Func<TEvent, Task> handler, 
        string routingKey = "*", 
        CancellationToken cancellationToken = default) 
        where TEvent : IDomainEvent;

    /// <summary>
    /// Suscribirse a eventos usando un routing key pattern
    /// </summary>
    /// <param name="handler">Manejador genérico del evento</param>
    /// <param name="routingKey">Routing key pattern</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task SubscribeAsync(
        Func<string, string, Task> handler, 
        string routingKey = "*", 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Iniciar la escucha de eventos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task StartListeningAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Detener la escucha de eventos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task StopListeningAsync(CancellationToken cancellationToken = default);
}