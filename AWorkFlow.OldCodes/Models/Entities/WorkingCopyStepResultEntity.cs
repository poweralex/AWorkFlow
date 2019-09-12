using Dapper.Contrib.Extensions;
using Mcs.SF.Common.ServiceProviders.CommonModel;
using System;

namespace Mcs.SF.WorkFlow.Api.Models.Entities
{
    /// <summary>
    /// entity model for result(s) of working step
    /// </summary>
    [Table("wf_working_step_result")]
    public class WorkingCopyStepResultEntity : GuidEntity
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
        /// is fail
        /// </summary>
        public bool Failed { get; set; }
        /// <summary>
        /// is cancelled
        /// </summary>
        public bool Cancelled { get; set; }
    }
}
