using Dapper.Contrib.Extensions;
using Mcs.SF.Common.ServiceProviders.CommonModel;

namespace Mcs.SF.WorkFlow.Api.Models.Entities
{
    /// <summary>
    /// entity model for workflow flow config
    /// </summary>
    [Table("wf_workflow_flow")]
    public class WorkFlowConfigFlowEntity : GuidEntity
    {
        /// <summary>
        /// workflow id
        /// </summary>
        public string WorkFlowId { get; set; }
        /// <summary>
        /// current step code
        /// </summary>
        public string CurrentStepCode { get; set; }
        /// <summary>
        /// next step code
        /// </summary>
        public string NextStepCode { get; set; }
        /// <summary>
        /// next condition
        /// </summary>
        public string NextOn { get; set; }
    }
}
