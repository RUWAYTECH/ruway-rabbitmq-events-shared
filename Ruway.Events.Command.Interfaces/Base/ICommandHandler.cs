using MediatR;

namespace Ruway.Events.Command.Interfaces.Base;

/// <summary>
/// Interface base para todos los manejadores de comandos
/// </summary>
/// <typeparam name="TCommand">Tipo del comando</typeparam>
/// <typeparam name="TResponse">Tipo de respuesta</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
}

/// <summary>
/// Interface base para manejadores de comandos sin respuesta
/// </summary>
/// <typeparam name="TCommand">Tipo del comando</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand
{
}