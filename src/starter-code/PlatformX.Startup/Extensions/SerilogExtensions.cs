using Amazon.CloudWatchLogs;
using Amazon.Runtime;
using Destructurama;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.AwsCloudWatch;
using System.Runtime.Serialization;

namespace PlatformX.Startup.Extensions
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
            var applicationName = Environment.GetEnvironmentVariable("APPLICATION_NAME") ?? "EMPTY";

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Default", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Destructure.UsingAttributes()
                .Enrich.WithProperty("EnvironmentName", environmentName)
                .Enrich.WithProperty("AspnetCoreEnvironmentName", aspNetCoreEnvironmentName)
                .Enrich.WithProperty("ApplicationName", applicationName)
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithMachineName()
                .Enrich.FromLogContext()
                .WriteTo.Console(new JsonFormatter());

            if (Environment.GetEnvironmentVariable("LOG_TO_CLOUDWATCH") == "true")
            {
                var logGroupName = $"{applicationName}/{environmentName}";

                var options = new CloudWatchSinkOptions
                {
                    // the name of the CloudWatch Log group for logging
                    LogGroupName = logGroupName,

                    // the main formatter of the log event
                    TextFormatter = new JsonFormatter(),

                    // other defaults defaults
                    MinimumLogEventLevel = LogEventLevel.Information,
                    BatchSizeLimit = 100,
                    QueueSizeLimit = 10000,
                    Period = TimeSpan.FromSeconds(10),
                    CreateLogGroup = true,
                    LogStreamNameProvider = new DefaultLogStreamProvider(),
                    RetryAttempts = 5
                };

                // setup AWS CloudWatch client
                var client = new AmazonCloudWatchLogsClient();

                loggerConfiguration.WriteTo.AmazonCloudWatch(options, client);
            }

            var logger = loggerConfiguration.CreateLogger();

            return logger;
        }
    }
}
