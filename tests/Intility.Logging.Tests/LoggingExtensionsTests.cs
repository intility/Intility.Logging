using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Intility.Logging.Tests;

public class LoggingExtensionsTests
{
    [Fact]
    public void CanCreateLoggerInstance()
    {
        // Arrange
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console();

        Log.Logger = loggerConfig.CreateLogger();

        // Act
        var logger = Log.Logger;

        // Assert
        Assert.NotNull(logger);
        logger.Information("Test log message");
    }

    [Fact]
    public void CanConfigureLoggingWithHostBuilder()
    {
        // Arrange & Act
        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<LoggingExtensionsTests>>();

        // Assert
        Assert.NotNull(logger);
        logger.LogInformation("Test log from host builder");
    }

    [Fact]
    public void LoggerCanWriteDifferentLevels()
    {
        // Arrange
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console();

        var logger = loggerConfig.CreateLogger();

        // Act & Assert
        Assert.NotNull(logger);

        logger.Verbose("Verbose message");
        logger.Debug("Debug message");
        logger.Information("Information message");
        logger.Warning("Warning message");
        logger.Error("Error message");
    }

    [Fact]
    public void LoggerCanWriteStructuredData()
    {
        // Arrange
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console();

        var logger = loggerConfig.CreateLogger();

        // Act & Assert
        Assert.NotNull(logger);

        logger.Information("User {UserId} logged in from {IpAddress}",
            12345,
            "192.168.1.1");
    }

    [Fact]
    public void LoggerMinimumLevelIsRespected()
    {
        // Arrange
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Warning()
            .WriteTo.Console();

        var logger = loggerConfig.CreateLogger();

        // Act & Assert
        Assert.NotNull(logger);
        Assert.True(logger.IsEnabled(LogEventLevel.Warning));
        Assert.True(logger.IsEnabled(LogEventLevel.Error));
        Assert.False(logger.IsEnabled(LogEventLevel.Debug));
        Assert.False(logger.IsEnabled(LogEventLevel.Verbose));
    }
}
