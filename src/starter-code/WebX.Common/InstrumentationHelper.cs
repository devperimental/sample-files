using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlatformX.Messaging.Types;
using PlatformX.Messaging.Types.Constants;
using PlatformX.ServiceLayer.Common.Behaviours;
using PlatformX.ServiceLayer.Common.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebX.Common.Shared.Behaviours;
using WebX.Common.Shared.Types;

namespace WebX.Common
{
    public class InstrumentationHelper : IInstrumentationHelper
    {
        private readonly ILogger<InstrumentationHelper> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ClientRequestM _clientRequest;
        public InstrumentationHelper(ILogger<InstrumentationHelper> logger,
            IHttpContextAccessor httpContextAccessor,
            ClientRequestM clientRequest)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _clientRequest = clientRequest;
        }

        public async Task<TResponseM> InstrumentRequest<TResponseM>(Func<Task<TResponseM>> func, string controller, string actionName, long responseSize = 0) where TResponseM : IBaseServiceResponseM, new()
        {
            var response = default(TResponseM);
            var success = false;
            var start = DateTime.UtcNow;

            try
            {
                response = await func();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "InstrumentRequest:Func");
                success = false;
                throw;
            }
            finally
            {
                await PerformInstrumentSave(response, controller, actionName, start, responseSize, success);
            }

            return response;
        }

        public async Task InstrumentRequest(Action action, string controller, string actionName, long responseSize = 0)
        {
            var response = default(BaseServiceResponseM);
            var success = false;
            var start = DateTime.UtcNow;

            try
            {
                action();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "InstrumentRequest:Action");
                success = false;
                throw;
            }
            finally
            {
                await PerformInstrumentSave(response, controller, actionName, start, responseSize, success);
            }
        }

        private async Task PerformInstrumentSave(IBaseServiceResponseM? response, string controller, string action, DateTime start, long responseSize, bool success)
        {
            var end = DateTime.UtcNow;
            var requestSize = _httpContextAccessor.HttpContext.Request.ContentLength ?? 0;
            var verb = _httpContextAccessor.HttpContext.Request.Method;

            if (responseSize == 0)
            {
                responseSize = response != null ? JsonConvert.SerializeObject(response).Length : 0;
            }

            var scope = new Dictionary<string, object> {
                { "Controller", controller },
                { "Action", action },
                { "Verb", verb },
                { "RequestSize", requestSize },
                { "ResponseSize", responseSize },
                { "CaptureStartDate", start },
                { "CaptureEndDate", end },
                { "Success", success },
                { "OrganisationGlobalId", _clientRequest.OrganisationGlobalId! },
                { "ApplicationKey", _clientRequest.ClientApplicationKey! },
                { "ApiKey", _clientRequest.ClientApiKey! },
                { "ApplicationGlobalId", _clientRequest.ClientApplicationGlobalId! },
                { "ApplicationEnvironment", _clientRequest.ClientAppEnvironment! },
                { "ApiRoleType", SystemApiRoleType.Client }
            };

            using (_logger.BeginScope(scope))
            {
                _logger.LogInformation("InstrumentationMessage {@response}", response);
            }

            await Task.CompletedTask;
            //await _apiTelemetryCaptureHelper.Instrument(response, controller, action, verb, requestSize, responseSize, start, end);
        }
    }
}