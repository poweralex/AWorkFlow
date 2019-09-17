using System;

namespace AWorkFlow.Core.Models
{
    /// <summary>
    /// Model of operation result
    /// </summary>
    public class OperationResult
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public OperationResult()
        {
        }

        /// <summary>
        /// constructor, copy from another OperationResult or OperationResult&lt;T&gt;
        /// </summary>
        /// <param name="result"></param>
        public OperationResult(OperationResult result)
        {
            Success = result.Success;
            Code = result.Code;
            Message = result.Message;
            Exception = result.Exception;
        }

        /// <summary>
        /// Indicates that the operation was successful or not
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Message description
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Exception
        /// </summary>
        public Exception Exception { get; set; }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    /// <typeparam name="T">type of Data</typeparam>
    public class OperationResult<T> : OperationResult
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public OperationResult() : base()
        {

        }

        /// <summary>
        /// constructor, copy from another OperationResult or OperationResult&lt;T&gt;
        /// </summary>
        /// <param name="result">copy from</param>
        public OperationResult(OperationResult result) : base(result)
        {

        }

        /// <summary>
        /// constructor, copy from another OperationResult which type is same as this object
        /// </summary>
        /// <param name="result">copy from</param>
        public OperationResult(OperationResult<T> result) : base(result)
        {
            Data = result.Data;
        }

        /// <summary>
        /// Data
        /// </summary>
        public T Data { get; set; }
    }
}
