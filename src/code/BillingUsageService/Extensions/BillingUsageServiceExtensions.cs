using Amazon;
using Amazon.CloudWatchLogs;
using DataAccess.Billing;
using DataAccess.Billing.Shared.Behaviours;
using DomainX.Portal.Shared.Behaviours;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlatformX.Common.Types.DataContract;
using PlatformX.DataAccess.Common.Behaviours;
using PlatformX.DataAccess.Common.Types;
using PlatformX.DataAccess.Helper;
using PlatformX.FileStore;
using PlatformX.FileStore.Shared.Behaviours;
using PlatformX.Http.Behaviours;
using PlatformX.Http.Helper;
using PlatformX.Messaging.Types;
using PlatformX.Secrets.Shared.Behaviours;
using PlatformX.Startup.Extensions;
using PlatformX.StorageProvider.Shared.Behaviours;
using PlatformX.Utility;
using PlatformX.Utility.Shared.Behaviours;
using BillingUsageService.Behaviours;
using BillingUsageService.Extensions;
using ProviderX.StorageProvider.S3;
using Repository.Billing;
using Repository.Billing.Shared.Behaviours;
using WebX.Common;
using WebX.Common.Shared.Behaviours;
using ServiceLayer.Portal;
using Repository.Portal;
using DataAccess.Portal;
using Providers.Portal;
using DataAccess.Portal.Shared;
using Repository.Portal.Shared.Behaviours;
using Providers.Portal.Shared.Behaviours;
using ServiceLayer.Portal.Shared;
using Settings.Portal.Config;
using Providers.Payment.Shared.Behaviours;
using Providers.Payment.Shared.Types;
using Providers.Payment.Stripe;

namespace BillingUsageService
{
    /// <summary>
    /// IdentityServiceExtensions
    /// </summary>
    public static class BillingUsageServiceExtensions
    {
        /// <summary>
        /// AddPortalService
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddBillableItemServiceLayer(this IServiceCollection services)
        {
            services.AddBootstrapConfiguration();
            services.AddSingleton<IConfigurationService, ConfigurationService>();

            services.AddAwsResources();
            services.BootstrapExternalDependancies();
            services.BootstrapSettings();

            services.AddScoped<IBaseDatabaseHelper, BaseDatabaseHelper>();
            services.AddScoped<IDatabaseHelper, DatabaseHelper>();

            services.AddScoped<IBillingDataAccess, BillingDataAccess>();
            services.AddScoped<IBillingRepository, BillingRepository>();
            services.AddScoped<IPortalDataAccess, PortalDataAccess>();
            services.AddScoped<IPortalRepository, PortalRepository>();
            services.AddScoped<IPortalProvider, PortalProvider>();

            services.AddScoped<RequestContext, RequestContext>();
            services.AddScoped<ICommonHelper, CommonHelper>();

            services.AddScoped(typeof(IStripeApiWrapper), c => {
                var scope = services.BuildServiceProvider();
                var stripeConfiguration = scope.GetRequiredService<StripeConfigurationSettings>();
                var logger = scope.GetRequiredService<ILogger<StripeApiWrapper>>();

                if (stripeConfiguration.StripeBypass)
                {
                    var bypassLogger = scope.GetRequiredService<ILogger<StripeApiBypassWrapper>>();
                    return new StripeApiBypassWrapper(stripeConfiguration, bypassLogger);
                }

                return new StripeApiWrapper(stripeConfiguration, logger);
            });

            services.AddScoped<IPaymentProvider, StripePaymentProvider>();
            services.AddScoped<IPortalService, PortalServiceLayer>();
        }

        public static void BootstrapExternalDependancies(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddHttpClient<IHttpRequestHelper, HttpRequestHelper>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(20);
            });

            services.AddScoped<IHttpContextHelper, HttpContextHelper>();
            services.AddScoped<IHttpRequestHelper, HttpRequestHelper>();
            services.AddScoped<IHashGenerator, HashGenerator>();

            services.AddScoped<IStorageProvider, S3StorageProvider>();
            services.AddScoped<IFileStore, DefaultFileStore>();

        }

        public static void BootstrapSettings(this IServiceCollection services)
        {
            var scope = services.BuildServiceProvider();
            var secretManager = scope.GetRequiredService<ISecretClient>();
            var bootstrapConfiguration = scope.GetRequiredService<BootstrapConfiguration>();
            var configurationService = scope.GetRequiredService<IConfigurationService>();
            var configuration = configurationService.GetConfiguration();

            services.AddSingleton(typeof(DBConfiguration), c =>
            {
                var config = new DBConfiguration { };
                configuration.GetSection(nameof(DBConfiguration)).Bind(config);

                if (bootstrapConfiguration.EnvironmentName != "Swagger" &&
                    bootstrapConfiguration.EnvironmentName != "local")
                {
                    config.ConnectionString = secretManager.GetSecret(config.ConnectionString);
                }
                else
                {
                    config.ConnectionString = Environment.GetEnvironmentVariable("portal-db-local");
                    //config.ConnectionString = Environment.GetEnvironmentVariable("portal-db-dev");
                }

                return config;
            });

            services.AddSingleton(typeof(PortalSettings), c =>
            {
                var config = new PortalSettings { };
                configuration.GetSection(nameof(PortalSettings)).Bind(config);

                if (!string.IsNullOrEmpty(bootstrapConfiguration.EnvironmentName) &&
                    bootstrapConfiguration.EnvironmentName != "Swagger" &&
                    bootstrapConfiguration.EnvironmentName != "local")
                {
                    config.TokenSecurityKey = secretManager.GetSecret(config.TokenSecurityKey);
                }
                else
                {
                    config.TokenSecurityKey = $"This is my cu5tom Secret key for Authentication and this is to satisfy the key length for HMAC512!";
                }

                return config;
            });

            services.AddSingleton(typeof(StripeConfigurationSettings), c =>
            {
                var config = new StripeConfigurationSettings { };
                configuration.GetSection(nameof(StripeConfigurationSettings)).Bind(config);

                return config;
            });

            services.AddSingleton(typeof(IPortalSettings), c =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var settings = serviceProvider.GetRequiredService<PortalSettings>();

                return settings;
            });
        }
    }
}
