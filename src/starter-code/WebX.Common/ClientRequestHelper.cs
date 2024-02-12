using Azure;
using DomainX.Portal.Shared.DataContract;
using Microsoft.Extensions.Logging;
using PlatformX.FileStore.Shared.Behaviours;
using PlatformX.FileStore.Shared.Types;
using PlatformX.Http.Behaviours;
using PlatformX.ServiceLayer.Common.Types;
using PlatformX.Utility.Shared.Behaviours;
using PlatformX.Utility.Shared.EnumTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using WebX.Common.Shared.Behaviours;
using WebX.Common.Shared.Types;

namespace WebX.Common
{
    public class ClientRequestHelper : IClientRequestHelper
    {
        private readonly IHttpContextHelper _httpContextHelper;
        private readonly IHashGenerator _hashGenerator;
        private readonly IFileStore _fileStore;
        private readonly ILogger _logger;

        private const string INVALID_API_CREDENTIALS = "INVALID_API_CREDENTIALS";
        private const string INVALID_APPLICATION_KEY = "INVALID_APPLICATION_KEY";
        private const string INVALID_APPLICATION_ENVIRONMENT = "INVALID_APPLICATION_ENVIRONMENT";
        public ClientRequestHelper(IHttpContextHelper httpContextHelper,
                IHashGenerator hashGenerator,
                IFileStore fileStore,
                ILogger<ClientRequestHelper> logger)
        {
            _httpContextHelper = httpContextHelper;
            _hashGenerator = hashGenerator;
            _fileStore = fileStore;
            _logger = logger;
        }

        public ClientRequestM BuildClientRequest(Dictionary<string, string> parameters)
        {
            var clientRequest = new ClientRequestM();

            // Grab the api-key
            var envList = new List<string> { "DEV", "UAT", "PROD" };
            var applicationKey = _httpContextHelper.GetHeaderKeyValue("x-app-key");
            var applicationEnvironment = _httpContextHelper.GetHeaderKeyValue("x-app-env");

            if (string.IsNullOrEmpty(applicationKey))
            {
                clientRequest.StatusCode = (int)HttpStatusCode.Forbidden;
                clientRequest.SubStatusCode = 1;
                clientRequest.ReasonPhrase = INVALID_APPLICATION_KEY;
                clientRequest.Valid = false;

                return clientRequest;
            }

            if (string.IsNullOrEmpty(applicationEnvironment))
            {
                clientRequest.StatusCode = (int)HttpStatusCode.Forbidden;
                clientRequest.SubStatusCode = 1;
                clientRequest.ReasonPhrase = INVALID_APPLICATION_ENVIRONMENT;
                clientRequest.Valid = false;

                return clientRequest;
            }

            var urlSafeRegexPattern = "^[a-zA-Z0-9_-]*$";
            var appIdMatch = Regex.IsMatch(applicationKey, urlSafeRegexPattern);

            if (!appIdMatch)
            {
                clientRequest.StatusCode = (int)HttpStatusCode.Forbidden;
                clientRequest.SubStatusCode = 2;
                clientRequest.ReasonPhrase = INVALID_APPLICATION_KEY;
                clientRequest.Valid = false;

                return clientRequest;
            }

            var validEnvironment = envList.Contains(applicationEnvironment);

            if (!validEnvironment)
            {
                clientRequest.StatusCode = (int)HttpStatusCode.Forbidden;
                clientRequest.SubStatusCode = 3;
                clientRequest.ReasonPhrase = INVALID_APPLICATION_ENVIRONMENT;
                clientRequest.Valid = false;

                return clientRequest;
            }

            //var apiKeyHash = _hashGenerator.CreateHash(apiKey, HashType.SHA256);

            var fileRequest = new FileStoreRequest { ContainerName = parameters["CONTAINERNAME"], FilePath = $"application/{applicationKey}/data.json" };
            
            try
            {
                var applicationData = _fileStore.LoadFile<ApplicationResponse>(fileRequest);

                if (applicationData == null ||
                    applicationData.Application == null)
                {
                    clientRequest.StatusCode = (int)HttpStatusCode.Forbidden;
                    clientRequest.SubStatusCode = 4;
                    clientRequest.ReasonPhrase = INVALID_APPLICATION_KEY;
                    clientRequest.Valid = false;

                    return clientRequest;
                }

                if (applicationData.ApplicationTokens?.Count <= 0)
                {
                    clientRequest.StatusCode = (int)HttpStatusCode.Forbidden;
                    clientRequest.SubStatusCode = 5;
                    clientRequest.ReasonPhrase = INVALID_APPLICATION_KEY;
                    clientRequest.Valid = false;

                    return clientRequest;
                }

                var contextToken = applicationData.ApplicationTokens.SingleOrDefault(c => c.EnvironmentKey?.ToLower() == applicationEnvironment.ToLower());

                if (contextToken == null)
                {
                    clientRequest.StatusCode = (int)HttpStatusCode.Forbidden;
                    clientRequest.SubStatusCode = 6;
                    clientRequest.ReasonPhrase = INVALID_APPLICATION_ENVIRONMENT;
                    clientRequest.Valid = false;

                    return clientRequest;
                }

                var apiKey = _httpContextHelper.GetHeaderKeyValue("x-api-key");
                var apiSecret = _httpContextHelper.GetHeaderKeyValue("x-api-secret");
             
                var validCredentials = ValidateClientCredentials(apiKey, apiSecret, contextToken);

                if (!validCredentials)
                {
                    clientRequest.StatusCode = (int)HttpStatusCode.Forbidden;
                    clientRequest.SubStatusCode = 7;
                    clientRequest.ReasonPhrase = INVALID_API_CREDENTIALS;
                    clientRequest.Valid = false;

                    return clientRequest;
                }

                clientRequest.IpAddress = _httpContextHelper.DetermineIpAddress();
                clientRequest.UserAgent = _httpContextHelper.DetermineUserAgent();

                clientRequest.ApplicationGlobalId = applicationData.Application.GlobalId;
                clientRequest.OrganisationGlobalId = applicationData.Application.OrganisationGlobalId;
                clientRequest.ClientApplicationKey = applicationKey;
                clientRequest.ClientApiKey = apiKey;
                clientRequest.ClientApplicationGlobalId = applicationData.Application.GlobalId;
                clientRequest.ClientAppEnvironment = applicationEnvironment;
                clientRequest.Valid = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BuildClientRequest for {@fileRequest}", fileRequest);
                clientRequest.Valid = false;
                clientRequest.ReasonPhrase = ex.InnerException?.Message;
            }
            
            return clientRequest;
        }

        public bool ValidateClientCredentials(string apiKey, string apiSecret, ApplicationToken applicationToken)
        {
            if (string.IsNullOrEmpty(apiKey) ||
                string.IsNullOrEmpty(apiSecret))
            {
                return false;
            }
            var apiKeyHash = _hashGenerator.CreateHash(apiKey, HashType.SHA512);

            if (apiKeyHash != applicationToken.ClientIdHash)
            {
                return false;
            }

            var apiSecretHash = _hashGenerator.CreateHash(apiSecret, HashType.SHA512);

            if (apiSecretHash != applicationToken.ClientSecretHash)
            {
                return false;
            }

            return true;
        }
    }
}
