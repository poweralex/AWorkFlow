using Dapper.Contrib.Extensions;
using Mcs.SF.Common.ServiceProviders.CommonModel;

namespace Mcs.SF.WorkFlow.Api.Models.Entities
{
    /// <summary>
    /// entity model of wf_status
    /// </summary>
    [Table("wf_status")]
    public class StatusEntity : GuidEntity
    {
        /// <summary>
        /// TargetId
        /// </summary>
        public string TargetId { get; set; }
        /// <summary>
        /// Status
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// Qty
        /// </summary>
        public int? Qty { get; set; }
    }
}
