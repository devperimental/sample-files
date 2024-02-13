using Destructurama;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Helper.Startup.Extensions
{
    public static class SerilogExtensions
    {
        public static void AddSerilog(this IServiceCollection services, ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.ClearProviders();

            var logger = ConfigureLogger();

            loggingBuilder.AddSerilog(logger);

            services.AddLogging(lb =>
            {
                lb.AddSerilog(Log.Logger, true);
            });
        }

        public static void AddSerilog(this IServiceCollection services)
        {
            var logger = ConfigureLogger();

            services.AddLogging(lb =>
            {
                lb.AddSerilog(Log.Logger, true);
            });
        }

        public static Logger ConfigureLogger()
        {
            var environmentName = Environment.GetEnvironmentVariable("ENVIRONMENT_NAME") ?? "EMPTY";
            var aspNetCoreEnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "EMPTY";
            var serviceName = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "EMPTY";

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Default", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Destructure.UsingAttributes()
                .Enrich.WithProperty("EnvironmentName", environmentName)
                .Enrich.WithProperty("AspnetCoreEnvironmentName", aspNetCoreEnvironmentName)
                .Enrich.WithProperty("ServiceName", serviceName)
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithMachineName()
                .Enrich.FromLogContext()
                .WriteTo.Console(new JsonFormatter());

            var logger = loggerConfiguration.CreateLogger();

            return logger;
        }
    }
}
