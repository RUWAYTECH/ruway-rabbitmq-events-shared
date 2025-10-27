using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Ruway.Events.Command.Configuration;
using Ruway.Events.Command.Interfaces.Events;
using EventSubscriberConsoleApp.Data;
using EventSubscriberConsoleApp.Services;

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
            EmployeeEventsRoutingKey = "employee.events",
            EntityRoutingKeys = new Dictionary<string, string>
            {
                { "employee", "memos.employee.events" },
                { "enterprise", "memos.enterprise.events" },
                { "store", "memos.store.events" },
                { "employee_store", "memos.employee_store.events" }
            }
        });

        // Configurar conexión a base de datos
        var connectionString = "Server=172.16.10.12;Database=DBSecurityQA;User=memo;Password=Memo$2025;TrustServerCertificate=True;";
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        // Registrar servicios
        services.AddScoped<IPasswordService, PasswordService>();

        var serviceProvider = services.BuildServiceProvider();
        var subscriber = serviceProvider.GetRequiredService<IEventSubscriber>();
        var publisher = serviceProvider.GetRequiredService<IEventPublisher>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("=== Iniciando Sistema de Eventos de Empleados ===");

            try
            {
                logger.LogInformation("🔧 Configurando suscripciones...");

                // 1. Configurar suscripción específica para eventos de creación de empleados
                await subscriber.SubscribeAsync<EmployeeCreatedEvent>(async (employeeCreatedEvent) =>
                {
                    logger.LogInformation("🎉 Nuevo empleado creado: {FirstName} {LastName} ({Email})",
                        employeeCreatedEvent.FirstName, employeeCreatedEvent.LastName, employeeCreatedEvent.Email);

                    await ProcessNewEmployeeCreated(employeeCreatedEvent, logger);
                }, "memos.employee.events.created");

                logger.LogInformation("✅ Suscripción específica configurada para 'memos.employee.events.created'");

                // 2. Suscripción genérica para logging de todos los eventos de empleados
                await subscriber.SubscribeAsync(async (message, routingKey) =>
                 {
                     logger.LogInformation("📨 Evento de empleado [{RoutingKey}]: {Message}",
                         routingKey, message);
                     await Task.CompletedTask;
                 }, "memos.employee.events.*");

                logger.LogInformation("✅ Suscripción genérica configurada para 'memos.employee.events.*'");

                logger.LogInformation("🔄 Iniciando escucha de eventos...");
                await subscriber.StartListeningAsync();
                logger.LogInformation("✅ Escucha de eventos iniciada correctamente");
            }
            catch (Exception connectionEx)
            {
                logger.LogWarning(connectionEx, "⚠️ No se pudo conectar a RabbitMQ. La aplicación funcionará en modo publicación únicamente.");
            }

            logger.LogInformation("✅ Sistema iniciado.");
            logger.LogInformation("📝 Comandos disponibles:");
            logger.LogInformation("   - 'create' para crear un empleado de prueba");
            logger.LogInformation("   - 'help' para mostrar ayuda");
            logger.LogInformation("   - 'exit' para salir");
            logger.LogInformation("");
            logger.LogInformation("💡 Escribe un comando y presiona Enter:");

            // Bucle de comandos interactivos
            await ProcessUserCommands(publisher, logger, serviceProvider);

            try
            {
                await subscriber.StopListeningAsync();
                logger.LogInformation("✅ Sistema detenido correctamente");
            }
            catch (Exception stopEx)
            {
                logger.LogWarning(stopEx, "⚠️ Error al detener suscriptor (probablemente no estaba conectado)");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error crítico en el sistema de eventos");
        }
        finally
        {
            subscriber.Dispose();
        }
    }

    /// <summary>
    /// Procesa comandos del usuario para crear empleados o salir
    /// </summary>
    private static async Task ProcessUserCommands(IEventPublisher publisher, ILogger logger, IServiceProvider serviceProvider)
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

                case "update-passwords":
                case "migrate-passwords":
                    await UpdateAllUserPasswords(serviceProvider, logger);
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
            var personalEmail = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com";

            var employeeCreatedEvent = new EmployeeCreatedEvent(
                employeeId,
                firstName,
                lastName,
                documentNumber,
                email,
                phone,
                personalEmail,
                EmployeeStores: new[]
                {
                    new EmployeeStores(Guid.NewGuid(), DateTime.UtcNow)
                }
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
    /// Recorre todos los usuarios y actualiza su PasswordHash (inicialmente con el username)
    /// </summary>
    private static async Task UpdateAllUserPasswords(IServiceProvider serviceProvider, ILogger logger)
    {
        logger.LogInformation("🔁 Iniciando migración de passwords para todos los usuarios...");

        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pwdService = scope.ServiceProvider.GetRequiredService<IPasswordService>();

        var users = await db.Users.Where(a => a.UserName != "admin").ToListAsync();
        logger.LogInformation("ℹ️ Usuarios encontrados: {Count}", users.Count);

        var updated = 0;
        foreach (var user in users)
        {
            try
            {
                // If passwordhash is empty or null, set to hash of username
                if (string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    user.PasswordHash = pwdService.HashPassword(user.UserName);
                    user.UpdatedAt = DateTime.UtcNow;
                    updated++;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error actualizando password para usuario {UserId}", user.UserId);
            }
        }

        if (updated > 0)
        {
            await db.SaveChangesAsync();
            logger.LogInformation("✅ Passwords migrados: {Count}", updated);
        }
        else
        {
            logger.LogInformation("✅ No se encontraron passwords a migrar");
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
