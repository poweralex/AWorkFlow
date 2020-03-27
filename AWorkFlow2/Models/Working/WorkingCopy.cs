using System;
using System.Collections.Generic;

namespace AWorkFlow2.Models.Working
{
    /// <summary>
    /// data model for working copy
    /// </summary>
    public class WorkingCopy : WorkingModelBase
    {
        /// <summary>
        /// working copy id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// workflow code
        /// </summary>
        public string WorkFlowCode { get; set; }
        /// <summary>
        /// workflow version
        /// </summary>
        public int? WorkFlowVersion { get; set; }
        /// <summary>
        /// begin time
        /// </summary>
        public DateTime? BeginTime { get; set; }
        /// <summary>
        /// end time
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// next execute time
        /// </summary>
        public DateTime? NextExecuteTime { get; set; }
        /// <summary>
        /// if this work is finished
        /// </summary>
        public bool IsFinished { get; set; }
        /// <summary>
        /// if this work is cancelled
        /// </summary>
        public bool IsCancelled { get; set; }
        /// <summary>
        /// is on-hold
        /// </summary>
        public bool OnHold { get; set; }
        /// <summary>
        /// on-hold time
        /// </summary>
        public DateTime? HoldTime { get; set; }
        /// <summary>
        /// release time
        /// </summary>
        public DateTime? ReleaseTime { get; set; }
        /// <summary>
        /// working arguments of the work(input/output)
        /// </summary>
        [IgnoreTracking]
        public WorkingArguments Arguments { get; set; }
        /// <summary>
        /// steps
        /// </summary>
        [IgnoreTracking]
        public List<WorkingCopyStep> Steps { get; set; }
        /// <summary>
        /// flows
        /// </summary>
        [IgnoreTracking]
        public List<WorkingCopyFlow> Flows { get; set; }
        /// <summary>
        /// groups of steps
        /// </summary>
        [IgnoreTracking]
        public List<WorkingCopyGroup> Groups { get; set; }
    }
}