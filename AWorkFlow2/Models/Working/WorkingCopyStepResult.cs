using System;

namespace AWorkFlow2.Models.Working
{
    /// <summary>
    /// data model for working step execute result
    /// </summary>
    public class WorkingCopyStepResult : WorkingModelBase
    {
        public string Id { get; set; }
        /// <summary>
        /// working step id
        /// </summary>
        public string WorkingStepId { get; set; }
        /// <summary>
        /// submit time
        /// </summary>
        public DateTime? SubmitTime { get; set; }
        /// <summary>
        /// qty
        /// </summary>
        public int? Qty { get; set; }
        /// <summary>
        /// is success
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// is failed
        /// </summary>
        public bool Failed { get; set; }
        /// <summary>
        /// is cancelled
        /// </summary>
        public bool Cancelled { get; set; }
        /// <summary>
        /// if this result posted next
        /// </summary>
        public bool PostedNext { get; set; }
        /// <summary>
        /// working arguments of the execution(input/output)
        /// </summary>
        public WorkingArguments Arguments { get; set; }
    }
}
