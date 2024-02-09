using DomainX.Shared.DataContract;
using FluentValidation;
using PlatformX.ServiceLayer.Common.Behaviours;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebX.Common.Shared.Behaviours;
using WebX.Common.Shared.Constants;

namespace WebX.Common
{
    public class CommonHelper : ICommonHelper
    {
        public CommonHelper()
        {
       
        }

        public TRes InitRequestModel<TV, TReq, TRes>(TReq model, string correlationId) where TV : AbstractValidator<TReq>, new() where TRes : IBaseServiceResponseM, new()
        {
            var response = new TRes()
            {
                CorrelationId = correlationId
            };

            var validator = new TV();

            if (model == null)
            {
                response.InError = true;
                response.ErrorType = ErrorType.VALIDATION;
                response.Messages = new List<string> { { MessageType.EmptyRequest } };
            }
            else
            {
                var validationOutcome = validator.Validate(model);

                if (validationOutcome.IsValid) return response;

                response.InError = true;
                response.ErrorType = ErrorType.VALIDATION;
                response.Messages = validationOutcome.Errors.ConvertAll(c=> c.ErrorCode);
            }

            return response;
        }

        public TModel Read<TContract, TModel>(TContract contract) where TModel : new()
        {
            var x = new TModel();

            if (contract != null)
            {
                var props = contract.GetType().GetProperties();

                foreach (var prop in props)
                {
                    try
                    {
                        var item = prop.GetValue(contract);
                        if (item == null)
                        {
                            continue;
                        }

                        var propertyName = prop.Name;

                        var modelProp = x.GetType().GetProperty(propertyName);

                        if (modelProp == null) continue;

                        switch (item)
                        {
                            case string _:
                            case decimal _:
                            case float _:
                            case long _:
                            case int _:
                            case bool _:
                            case DateTime _:
                                modelProp.SetValue(x, item);
                                break;
                            case Guid _:
                                modelProp.SetValue(x, Convert.ToString(item));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException($"Error mapping property: {prop.Name}", ex);
                    }
                }
            }


            return x;
        }

        public async Task<TResponseM> BaseGet<TRequestM, TResponseM, TValidator, TR>(TRequestM model,
            Func<TResponseM, TR> func,
            Func<GenericResponse, string, Task> auditAction,
            string callerName,
            string correlationId)
            where TRequestM : IGetRequestM, new()
            where TValidator : AbstractValidator<TRequestM>, new()
            where TResponseM : IBaseServiceResponseM, new()
            where TR : GenericResponse, new()
        {
            var response = InitRequestModel<TValidator, TRequestM, TResponseM>(model, correlationId);

            if (response.InError)
            {
                return await Task.FromResult(response);
            }

            var result = func(response);

            await auditAction(result, callerName);

            return response;
        }

        public async Task<TResponseM> BaseSave<TRequestM, TResponseM, TValidator, TServiceResponse>(TRequestM model,
            Func<TRequestM, TServiceResponse> func,
            Func<GenericResponse, string, Task> auditAction,
            string callerName,
            string correlationId)
            where TRequestM : new()
            where TValidator : AbstractValidator<TRequestM>, new()
            where TResponseM : IBaseServiceResponseM, new()
            where TServiceResponse : GenericResponse, new()
        {
            var response = InitRequestModel<TValidator, TRequestM, TResponseM>(model, correlationId);

            if (response.InError)
            {
                return await Task.FromResult(response);
            }

            var result = func(model);

            await auditAction(result, callerName);

            if (result.Success) return await Task.FromResult(response);

            response.ErrorType = result.FailureReason;
            response.InError = !result.Success;
            response.Messages = new List<string> { result.Message! };

            return await Task.FromResult(response);
        }

    }
}