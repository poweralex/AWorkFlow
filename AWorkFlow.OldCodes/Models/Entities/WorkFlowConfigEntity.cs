using Dapper.Contrib.Extensions;
using Mcs.SF.Common.ServiceProviders.CommonModel;

namespace Mcs.SF.WorkFlow.Api.Models.Entities
{
    /// <summary>
    /// entity model for workflow config
    /// </summary>
    [Table("wf_workflow")]
    public class WorkFlowConfigEntity : GuidEntity
    {
        /// <summary>
        /// workflow code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// workflow name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// workflow description
        /// </summary>
        public string Desc { get; set; }
        /// <summary>
        /// version
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// category
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// output setting
        /// </summary>
        public string Output { get; set; }
        /// <summary>
        /// is active
        /// </summary>
        public bool Active { get; set; }
    }
}
