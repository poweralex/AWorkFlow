using Dapper.Contrib.Extensions;
using Mcs.SF.Common.ServiceProviders.CommonModel;

namespace Mcs.SF.WorkFlow.Api.Models.Entities
{
    /// <summary>
    /// entity model for actions in workflow
    /// </summary>
    [Table("wf_workflow_action")]
    public class WorkFlowConfigActionEntity : GuidEntity
    {
        /// <summary>
        /// reference id
        /// </summary>
        public string RefId { get; set; }
        /// <summary>
        /// code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// action type
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// sequence
        /// </summary>
        public int Sequence { get; set; }
        /// <summary>
        /// action config json
        /// </summary>
        public string ActionConfig { get; set; }
    }
}
