using MediatR;

namespace Rokys.Events.Command.Interfaces.Base;

/// <summary>
/// Interface base para todos los comandos del sistema
/// </summary>
/// <typeparam name="TResponse">Tipo de respuesta del comando</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
    /// <summary>
    /// Identificador único del comando
    /// </summary>
    Guid CommandId { get; }
    
    /// <summary>
    /// Timestamp de cuando se creó el comando
    /// </summary>
    DateTime CreatedAt { get; }
    
    /// <summary>
    /// Usuario que ejecuta el comando
    /// </summary>
    string? UserId { get; }
}

/// <summary>
/// Interface base para comandos sin respuesta
/// </summary>
public interface ICommand : IRequest
{
    /// <summary>
    /// Identificador único del comando
    /// </summary>
    Guid CommandId { get; }
    
    /// <summary>
    /// Timestamp de cuando se creó el comando
    /// </summary>
    DateTime CreatedAt { get; }
    
    /// <summary>
    /// Usuario que ejecuta el comando
    /// </summary>
    string? UserId { get; }
}