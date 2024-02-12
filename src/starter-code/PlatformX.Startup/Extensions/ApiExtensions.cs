using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PlatformX.Common.Constants;
using PlatformX.Common.Types.DataContract;
using System.Reflection;

namespace PlatformX.Startup.Extensions
{
    public static class ApiExtensions
    {
        /// <summary>
        /// Add Container Service
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="loggingBuilder"></param>
        /// <param name="bootstrapConfiguration"></param>
        public static void AddContainerService(this IServiceCollection services, 
            IConfiguration configuration, 
            ILoggingBuilder loggingBuilder,
            BootstrapConfiguration bootstrapConfiguration)
        {
            services.AddOpenApiInfo(configuration);

            var scope = services.BuildServiceProvider();
            var openApiInfo = scope.GetRequiredService<OpenApiInfo>();

            services.AddSerilog(loggingBuilder);
            services.AddApiServiceCore(openApiInfo, bootstrapConfiguration);

        }

        /// <summary>
        /// AddLambdaService
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="bootstrapConfiguration"></param>
        public static void AddLambdaService(this IServiceCollection services,
            IConfiguration configuration,
            BootstrapConfiguration bootstrapConfiguration)
        {
            services.AddOpenApiInfo(configuration);

            var scope = services.BuildServiceProvider();
            var openApiInfo = scope.GetRequiredService<OpenApiInfo>();

            services.AddSerilog();
            services.AddApiServiceCore(openApiInfo, bootstrapConfiguration);
        }

        public static void AddOpenApiInfo(this IServiceCollection services, IConfiguration configuration)
        {
            var openApiInfo = new OpenApiInfo { };
            configuration.GetSection(nameof(OpenApiInfo)).Bind(openApiInfo);
            services.AddSingleton(openApiInfo);
        }

        public static void AddApiServiceCore(this IServiceCollection services, 
            OpenApiInfo openApiInfo,
            BootstrapConfiguration bootstrapConfiguration)
        {
            if (!string.IsNullOrEmpty(bootstrapConfiguration.CorsOrigins))
            {
                services.AddCors(options =>
                {
                    options.AddDefaultPolicy(
                        builder =>
                        {
                            builder
                                .SetIsOriginAllowedToAllowWildcardSubdomains()
                                .WithOrigins(bootstrapConfiguration.CorsOrigins.Split(','))
                                .AllowAnyMethod()
                                .AllowAnyHeader()
                                .AllowCredentials();
                        });
                });
            }

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;
            });

            if (bootstrapConfiguration.EnvironmentName != EnvironmentName.Production)
            {
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(c =>
                {
                    if (openApiInfo != null)
                    {
                        c.SwaggerDoc(openApiInfo.Version, openApiInfo);
                    }

                    var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

                    if (File.Exists(xmlFileName))
                    {
                        c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
                    }

                    c.EnableAnnotations();
                });
            }

            services.AddHealthChecks();
        }
    }
}
