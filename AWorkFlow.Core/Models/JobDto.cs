using System;
using System.Collections.Generic;

namespace AWorkFlow.Core.Models
{
    /// <summary>
    /// job dto
    /// </summary>
    public class JobDto
    {
        /// <summary>
        /// job unique id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// job type
        /// </summary>
        public JobTypes JobType { get; set; }
        public string WorkId { get; set; }
        public string WorkStepId { get; set; }
        public DateTime ActiveTime { get; set; }
        public bool Completed { get; set; }
        public bool Success { get; set; }
        public bool Fail { get; set; }
        public List<ActionSettingDto> Actions { get; set; }
        public Dictionary<string, string> PublicVariables { get; set; }
        public Dictionary<string, string> PrivateVariables { get; set; }
    }

    /// <summary>
    /// job types
    /// </summary>
    public enum JobTypes
    {
        /// <summary>
        /// PreAction of a work
        /// </summary>
        WorkPreAction,
        /// <summary>
        /// AfterAction of a work
        /// </summary>
        WorkAfterAction,
        /// <summary>
        /// PreAction of a step
        /// </summary>
        StepPreAction,
        /// <summary>
        /// Action of a step
        /// </summary>
        StepAction,
        /// <summary>
        /// AfterAction of a step
        /// </summary>
        StepAfterAction
    }
}
