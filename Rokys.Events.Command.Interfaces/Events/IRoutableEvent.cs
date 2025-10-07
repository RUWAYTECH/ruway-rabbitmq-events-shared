namespace Rokys.Events.Command.Interfaces.Events;

/// <summary>
/// Interface para eventos que pueden definir su propia routing key
/// </summary>
public interface IRoutableEvent
{
    /// <summary>
    /// Routing key específica para este evento
    /// </summary>
    string RoutingKey { get; }
}