using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ruway.Events.Command.Configuration;
using Ruway.Events.Command.Interfaces.Events;

namespace EventSubscriberConsoleApp;

/// <summary>
/// Aplicación de ejemplo para publicar y suscribirse a eventos de creación de empleados
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Configurar servicios
        var services = new ServiceCollection();
        
        // Configurar logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Configurar eventos con RabbitMQ
        services.AddEventPublisher(new RabbitMQSettings
        {
            HostName = "172.16.10.12",
            Port = 5672,
            UserName = "owner",
            Password = "P4ss@78_#%a9",
            EventsExchange = "ruway.events",
            EmployeeEventsRoutingKey = "employee.events"
        });
        
        var serviceProvider = services.BuildServiceProvider();
        var subscriber = serviceProvider.GetRequiredService<IEventSubscriber>();
        var publisher = serviceProvider.GetRequiredService<IEventPublisher>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            logger.LogInformation("=== Iniciando Sistema de Eventos de Empleados ===");
            
            // 1. Configurar suscripción específica para eventos de creación de empleados
            await subscriber.SubscribeAsync<EmployeeCreatedEvent>(async (employeeCreatedEvent) =>
            {
                logger.LogInformation("🎉 Nuevo empleado creado: {FirstName} {LastName} ({Email})",
                    employeeCreatedEvent.FirstName, employeeCreatedEvent.LastName, employeeCreatedEvent.Email);
                    
                await ProcessNewEmployeeCreated(employeeCreatedEvent, logger);
            }, "memos.employee.created");
            
            // 2. Suscripción genérica para logging de todos los eventos de empleados
            await subscriber.SubscribeAsync(async (message, routingKey) =>
            {
                logger.LogInformation("� Evento de empleado [{RoutingKey}]: {Message}", 
                    routingKey, message);
                await Task.CompletedTask;
            }, "employee.events.*");
            
            logger.LogInformation("🔄 Iniciando escucha de eventos...");
            await subscriber.StartListeningAsync();
            
            logger.LogInformation("✅ Suscriptor iniciado.");
            logger.LogInformation("📝 Comandos disponibles:");
            logger.LogInformation("   - 'create' para crear un empleado de prueba");
            logger.LogInformation("   - 'exit' para salir");
            
            // Bucle de comandos interactivos
            await ProcessUserCommands(publisher, logger);
            
            await subscriber.StopListeningAsync();
            logger.LogInformation("✅ Sistema detenido correctamente");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error en el sistema de eventos");
        }
        finally
        {
            subscriber.Dispose();
        }
    }
    
    /// <summary>
    /// Procesa comandos del usuario para crear empleados o salir
    /// </summary>
    private static async Task ProcessUserCommands(IEventPublisher publisher, ILogger logger)
    {
        while (true)
        {
            Console.Write("\n> ");
            var command = Console.ReadLine()?.ToLower().Trim();
            
            switch (command)
            {
                case "create":
                    await CreateSampleEmployee(publisher, logger);
                    break;
                    
                case "exit":
                case "quit":
                case "q":
                    logger.LogInformation("🛑 Cerrando aplicación...");
                    return;
                    
                case "help":
                case "?":
                    ShowHelp(logger);
                    break;
                    
                default:
                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        logger.LogWarning("❓ Comando desconocido: '{command}'. Escribe 'help' para ver los comandos disponibles.", command);
                    }
                    break;
            }
        }
    }
    
    /// <summary>
    /// Crea y publica un evento de empleado de prueba
    /// </summary>
    private static async Task CreateSampleEmployee(IEventPublisher publisher, ILogger logger)
    {
        try
        {
            // Generar datos de empleado aleatorios
            var employeeId = Guid.NewGuid();
            var names = new[] { "Juan", "María", "Carlos", "Ana", "Luis", "Carmen", "José", "Isabel" };
            var lastNames = new[] { "García", "Rodríguez", "López", "Martín", "Pérez", "González", "Sánchez", "Ruiz" };
            
            var random = new Random();
            var firstName = names[random.Next(names.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var documentNumber = $"{random.Next(10000000, 99999999)}";
            var email = $"{firstName.ToLower()}.{lastName.ToLower()}@ruway.com";
            var phone = $"6{random.Next(10000000, 99999999)}";
            
            var employeeCreatedEvent = new EmployeeCreatedEvent(
                employeeId,
                firstName,
                lastName,
                documentNumber,
                email,
                phone
            );
            
            logger.LogInformation("📤 Publicando evento de creación de empleado...");
            logger.LogInformation("   ID: {EmployeeId}", employeeId);
            logger.LogInformation("   Nombre: {FirstName} {LastName}", firstName, lastName);
            logger.LogInformation("   Email: {Email}", email);
            
            await publisher.PublishAsync(employeeCreatedEvent);
            
            logger.LogInformation("✅ Evento de empleado creado publicado correctamente");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error al crear y publicar evento de empleado");
        }
    }
    
    /// <summary>
    /// Procesa un evento de empleado creado recibido
    /// </summary>
    private static async Task ProcessNewEmployeeCreated(EmployeeCreatedEvent employeeEvent, ILogger logger)
    {
        try
        {
            logger.LogInformation("🔄 Procesando nuevo empleado...");
            
            // Simular procesamiento asíncrono
            await Task.Delay(500);
            
            logger.LogInformation("   ✅ Enviando email de bienvenida a {Email}", employeeEvent.Email);
            await Task.Delay(200);
            
            logger.LogInformation("   ✅ Creando cuenta de usuario para {FirstName} {LastName}", 
                employeeEvent.FirstName, employeeEvent.LastName);
            await Task.Delay(300);
            
            logger.LogInformation("   ✅ Notificando a RRHH sobre nuevo empleado {DocumentNumber}", 
                employeeEvent.DocumentNumber);
            await Task.Delay(200);
            
            logger.LogInformation("   ✅ Actualizando sistema de directorio corporativo");
            await Task.Delay(100);
            
            logger.LogInformation("🎉 Procesamiento completo para empleado {FirstName} {LastName}", 
                employeeEvent.FirstName, employeeEvent.LastName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error procesando evento de empleado creado: {EmployeeId}", 
                employeeEvent.EmployeeId);
        }
    }
    
    /// <summary>
    /// Muestra la ayuda de comandos disponibles
    /// </summary>
    private static void ShowHelp(ILogger logger)
    {
        logger.LogInformation("📋 Comandos disponibles:");
        logger.LogInformation("   create  - Crea y publica un evento de empleado de prueba");
        logger.LogInformation("   exit    - Sale de la aplicación");
        logger.LogInformation("   help    - Muestra esta ayuda");
    }
}
