using DomainX.Shared.DataContract;
using FluentValidation;
using PlatformX.ServiceLayer.Common.Behaviours;
using System;
using System.Threading.Tasks;

namespace WebX.Common.Shared.Behaviours
{
    public interface ICommonHelper
    {
        TRes InitRequestModel<TV, TReq, TRes>(TReq model, string correlationId) where TV : AbstractValidator<TReq>, new() where TRes : IBaseServiceResponseM, new();

        TModel Read<TContract, TModel>(TContract contract) where TModel : new();

        Task<TResponseM> BaseSave<TRequestM, TResponseM, TValidator, TServiceResponse>(TRequestM model,
            Func<TRequestM, TServiceResponse> func,
            Func<GenericResponse, string, Task> auditAction,
            string callerName,
            string correlationId)
            where TRequestM : new()
            where TValidator : AbstractValidator<TRequestM>, new()
            where TResponseM : IBaseServiceResponseM, new()
            where TServiceResponse : GenericResponse, new();

        Task<TResponseM> BaseGet<TRequestM, TResponseM, TValidator, TR>(TRequestM model,
            Func<TResponseM, TR> func,
            Func<GenericResponse, string, Task> auditAction,
            string callerName,
            string correlationId)
            where TRequestM : IGetRequestM, new()
            where TValidator : AbstractValidator<TRequestM>, new()
            where TResponseM : IBaseServiceResponseM, new()
            where TR : GenericResponse, new();


        //Task<TResponse> InstrumentRequest<TResponse>(Func<Task<TResponse>> func, string controller, string action)
        //    where TResponse : IResponseM, new();

        //Task InstrumentRequest(Action action, string controller, string actionName);
    }
}
