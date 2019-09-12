using Dapper.Contrib.Extensions;
using Mcs.SF.Common.ServiceProviders.CommonModel;
using System;

namespace Mcs.SF.WorkFlow.Api.Models.Entities
{
    /// <summary>
    /// entity model for working step
    /// </summary>
    [Table("wf_working_step")]
    public class WorkingCopyStepEntity : GuidEntity
    {
        /// <summary>
        /// working copy id
        /// </summary>
        public string WorkingCopyId { get; set; }
        /// <summary>
        /// previous step id
        /// </summary>
        public string PreviousWorkingCopyStepId { get; set; }
        /// <summary>
        /// step code from workflow config
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// step name from workflow config
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// status
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// status scope
        /// </summary>
        public string StatusScope { get; set; }
        /// <summary>
        /// status id
        /// </summary>
        public string StatusId { get; set; }
        /// <summary>
        /// tags
        /// </summary>
        public string Tags { get; set; }
        /// <summary>
        /// group
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// require to post next after this step finished
        /// </summary>
        public bool ActiveNext { get; set; }
        /// <summary>
        /// active time
        /// </summary>
        public DateTime? ActiveTime { get; set; }
        /// <summary>
        /// finished time
        /// </summary>
        public DateTime? FinishedTime { get; set; }
        /// <summary>
        /// is success
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// is normal finished
        /// </summary>
        public bool Finished { get; set; }
        /// <summary>
        /// is cancelled
        /// </summary>
        public bool Cancelled { get; set; }
        /// <summary>
        /// output json
        /// </summary>
        public string Output { get; set; }
    }
}
