using System;
using System.Collections.Generic;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class DismembermentViewModel
    {
        public bool IsDraft { get; set; }
        public string FamilyName { get; set; }
        public string OtherFamilyName { get; set; }
        public string OtherFamilyId { get; set; }
        public DateTime? DismembermentDate { get; set; }
        public decimal AllowancesAmount { get; set; }
        public string IsMainFamily { get; set; }
        public string NonFamilyRelationship { get; set; }
        public string FamilyCardPath { get; set; }
        public string Remarks { get; set; }
    }
    public sealed class DivorceViewMember
    {
        public string PartnerName { get; set; }
        public Guid? PartnerId { get; set; }
        public DateTime? DivorceDate { get; set; }
    }

    public static partial class DivorceErrors
    {
        public static Error AlreadyRequested(string spouse, string documentNumber)
        {
            return Error.BadRequest($"Divorce document already exists for spouse {spouse} under document number {documentNumber}");
        }
    }
    public sealed class Error
    {
        private Error(string message, int status)
        {
            Message = message;
            Status = status;
        }

        public string Message { get; }
        public int Status { get; }

        public static Error Create(string message, int status)
        {
            return new Error(message, status);
        }

        public static Error BadRequest(string message)
        {
            return new Error(message, 400);
        }
        public static Error NotFound(string message)
        {
            return new Error(message, 404);
        }
        
    }
    public sealed class Result<T> where T : class
    {
        private readonly T _data;
        private readonly Error _error;
        public bool IsError { get; set; }
        private Result(T data)
        {
            _data = data;
        }
        private Result(Error error)
        {
            _error = error;
            IsError = true;
        }
        public static implicit operator Result<T>(Error error)
        {
            return new Result<T>(error);
        }
        public static implicit operator Result<T>(T data)
        {
            return new Result<T>(data);
        }
        public Tresult Match<Tresult>(Func<T, Tresult> success, Func<Error, Tresult> error)
        {
            if (IsError)
                return error(_error);
            return success(_data);
        }
    }
}
