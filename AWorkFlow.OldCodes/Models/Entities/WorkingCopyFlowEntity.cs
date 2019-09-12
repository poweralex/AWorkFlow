using Dapper.Contrib.Extensions;
using Mcs.SF.Common.ServiceProviders.CommonModel;

namespace Mcs.SF.WorkFlow.Api.Models.Entities
{
    /// <summary>
    /// entity model for working flow
    /// </summary>
    [Table("wf_working_flow")]
    public class WorkingCopyFlowEntity : GuidEntity
    {
        /// <summary>
        /// working copy id
        /// </summary>
        public string WorkingCopyId { get; set; }
        /// <summary>
        /// from step id
        /// </summary>
        public string CurrentStepId { get; set; }
        /// <summary>
        /// to step id
        /// </summary>
        public string NextStepId { get; set; }
    }
}
