using Dapper.Contrib.Extensions;
using Mcs.SF.Common.ServiceProviders.CommonModel;

namespace Mcs.SF.WorkFlow.Api.Models.Entities
{
    /// <summary>
    /// entity model for workflow step config
    /// </summary>
    [Table("wf_workflow_step")]
    public class WorkFlowConfigStepEntity : GuidEntity
    {
        /// <summary>
        /// workflow id
        /// </summary>
        public string WorkFlowId { get; set; }
        /// <summary>
        /// step code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// desc
        /// </summary>
        public string Desc { get; set; }
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
        /// loop by
        /// </summary>
        public string LoopBy { get; set; }
        /// <summary>
        /// is manual step
        /// </summary>
        public bool Manual { get; set; }
        /// <summary>
        /// output config
        /// </summary>
        public string Output { get; set; }
        /// <summary>
        /// is begin step
        /// </summary>
        public bool IsBegin { get; set; }
        /// <summary>
        /// is end step
        /// </summary>
        public bool IsEnd { get; set; }
    }
}
