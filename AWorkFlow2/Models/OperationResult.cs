using System;

namespace AWorkFlow2.Models
{
    public class OperationResult
    {
        public OperationResult() { }
        public OperationResult(OperationResult res)
        {
            Success = res?.Success ?? false;
            Code = res?.Code;
            Message = res?.Message;
            Exception = res?.Exception;
        }

        public bool Success { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }

    public class OperationResult<T> : OperationResult
    {
        public OperationResult() { }
        public OperationResult(OperationResult res) : base(res) { }

        public T Data { get; set; }
    }
}
