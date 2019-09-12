using System;

namespace Mcs.SF.WorkFlow.Api.Models.Working
{
    /// <summary>
    /// data model for working step execute result
    /// </summary>
    public class WorkingCopyStepResult
    {
        /// <summary>
        /// working step id
        /// </summary>
        public string WorkingStepId { get; set; }
        /// <summary>
        /// submit time
        /// </summary>
        public DateTime? SubmitTime { get; set; }
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
    }
}
