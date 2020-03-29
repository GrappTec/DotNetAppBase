﻿using System;
using System.Linq;
using System.Threading.Tasks;
using DotNetAppBase.Std.Library.ComponentModel.Model.Svc.Enums;

namespace DotNetAppBase.Std.Library.ComponentModel.Model.Svc
{
    public class Result<TData>
    {
        public TData Data { get; set; }

        public string StatusMessage { get; set; }

        public EResultStatus Status { get; set; }

        public ResultDetail[] Details { get; set; }

        public bool Ok => Status == EResultStatus.Ok;

        public bool Fail => !Ok;

        public static Result<TData> Success(TData data) =>
            new Result<TData>
                {
                    Data = data,
                    Status = EResultStatus.Ok
                };

        public static async Task<Result<TData>> Success(Func<Task<TData>> funcTask)
        {
            var data = await funcTask();

            return Success(data);
        }

        public static Result<TData> Success(string success) =>
            new Result<TData>
                {
                    StatusMessage = success,
                    Status = EResultStatus.Ok
                };

        public static Result<TData> Exception(Exception ex) => Error(ex.Message);

        public static Result<TData> Error(string error) =>
            new Result<TData>
                {
                    StatusMessage = error,
                    Status = EResultStatus.Error
                };

        public static Result<TData> Error(ServiceResponse response)
        {
            return new Result<TData>
                {
                    StatusMessage = "Ocorreu um erro no processo, verifique o(s) detalhe(s),",
                    Status = response.Status == EServiceResponse.Succeeded ? EResultStatus.Ok : EResultStatus.Error,
                    Details = response.ValidationResult.Validations.Select(
                        validationResult =>
                            {
                                var key = validationResult.MemberNames.Any()
                                    ? validationResult.MemberNames.Aggregate((s, s1) => s + ";" + s1)
                                    : string.Empty;

                                return new ResultDetail
                                    {
                                        Key = key,
                                        Message = validationResult.ErrorMessage
                                    };
                            }).ToArray()
                };
        }

        public static Result<TData> Warning(string alert) =>
            new Result<TData>
                {
                    StatusMessage = alert,
                    Status = EResultStatus.Warning
                };
    }

    public class Result : Result<object>
    {
        public new static Result Success(object data) => new Result
            {
                Data = data,
                Status = EResultStatus.Ok
            };

        public new static async Task<Result> Success(Func<Task<object>> funcTask)
        {
            var data = await funcTask();

            return Success(data);
        }

        public new static Result Success(string success) =>
            new Result
            {
                StatusMessage = success,
                Status = EResultStatus.Ok
            };

        public new static Result Exception(Exception ex) => Error(ex.Message);

        public new static Result Error(string error) =>
            new Result
            {
                StatusMessage = error,
                Status = EResultStatus.Error
            };

        public new static Result Error(ServiceResponse response)
        {
            return new Result
            {
                StatusMessage = "Ocorreu um erro no processo, verifique o(s) detalhe(s),",
                Status = response.Status == EServiceResponse.Succeeded ? EResultStatus.Ok : EResultStatus.Error,
                Details = response.ValidationResult.Validations.Select(
                        validationResult =>
                        {
                            var key = validationResult.MemberNames.Any()
                                ? validationResult.MemberNames.Aggregate((s, s1) => s + ";" + s1)
                                : string.Empty;

                            return new ResultDetail
                            {
                                Key = key,
                                Message = validationResult.ErrorMessage
                            };
                        }).ToArray()
            };
        }

        public new static Result Warning(string alert) =>
            new Result
            {
                StatusMessage = alert,
                Status = EResultStatus.Warning
            };
    }
}