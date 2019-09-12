using System;
using System.Collections.Generic;

namespace Mcs.SF.WorkFlow.Api.Models.Working
{
    /// <summary>
    /// data model for working copy status
    /// </summary>
    public class WorkingCopyStatus
    {
        /// <summary>
        /// working copy id
        /// </summary>
        public string WorkingCopyId { get; set; }
        /// <summary>
        /// workflow code
        /// </summary>
        public string WorkFlowCode { get; set; }
        /// <summary>
        /// workflow version
        /// </summary>
        public int? WorkFlowVersion { get; set; }
        /// <summary>
        /// status
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// is normal finished
        /// </summary>
        public bool IsFinished { get; set; }
        /// <summary>
        /// is cancelled
        /// </summary>
        public bool IsCancelled { get; set; }
        /// <summary>
        /// begin time
        /// </summary>
        public DateTime? BeginTime { get; set; }
        /// <summary>
        /// end time
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// working copy output
        /// </summary>
        public object Output { get; set; }
        /// <summary>
        /// steps
        /// </summary>
        public List<WorkingCopyStepStatus> Steps { get; set; }
    }

    /// <summary>
    /// data model for working copy step status
    /// </summary>
    public class WorkingCopyStepStatus
    {
        /// <summary>
        /// working step id
        /// </summary>
        public string WorkingStepId { get; set; }
        /// <summary>
        /// previous working step id
        /// </summary>
        public string PreviousWorkingStepId { get; set; }
        /// <summary>
        /// step code
        /// </summary>
        public string StepCode { get; set; }
        /// <summary>
        /// begin time
        /// </summary>
        public DateTime? BeginTime { get; set; }
        /// <summary>
        /// end time
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// execute times count
        /// </summary>
        public int ExecuteCount { get; set; }
        /// <summary>
        /// is normal finished
        /// </summary>
        public bool IsFinished { get; set; }
        /// <summary>
        /// is success
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// is cancelled
        /// </summary>
        public bool IsCancelled { get; set; }
        /// <summary>
        /// step output
        /// </summary>
        public object Output { get; set; }
        /// <summary>
        /// execute results of the last execution
        /// </summary>
        public Dictionary<string, string> LastExecuteResults { get; set; }
    }
}
