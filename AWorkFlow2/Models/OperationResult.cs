using System;

namespace AWorkFlow2.Models
{
    public class OperationResult
    {
        public bool Success { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }

    public class OperationResult<T> : OperationResult
    {
        public T Data { get; set; }
    }
}
