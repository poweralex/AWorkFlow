using Dapper.Contrib.Extensions;
using Mcs.SF.Common.ServiceProviders.CommonModel;
using System;

namespace Mcs.SF.WorkFlow.Api.Models.Entities
{
    /// <summary>
    /// entity model for working copy
    /// </summary>
    [Table("wf_working")]
    public class WorkingCopyEntity : GuidEntity
    {
        /// <summary>
        /// workflow id
        /// </summary>
        public string WorkFlowId { get; set; }
        /// <summary>
        /// begin time
        /// </summary>
        public DateTime? BeginTime { get; set; }
        /// <summary>
        /// is normal finished
        /// </summary>
        public bool IsFinished { get; set; }
        /// <summary>
        /// is cancelled
        /// </summary>
        public bool IsCancelled { get; set; }
        /// <summary>
        /// endtime(finish or cancel)
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// output config
        /// </summary>
        public string Output { get; set; }
        /// <summary>
        /// is on-holding
        /// </summary>
        public bool OnHold { get; set; }
        /// <summary>
        /// hold time
        /// </summary>
        public DateTime? HoldTime { get; set; }
        /// <summary>
        /// release time
        /// </summary>
        public DateTime? ReleaseTime { get; set; }
    }
}
