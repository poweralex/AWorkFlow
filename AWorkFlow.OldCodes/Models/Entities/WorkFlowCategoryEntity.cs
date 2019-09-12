using Dapper.Contrib.Extensions;
using Mcs.SF.Common.ServiceProviders.CommonModel;

namespace Mcs.SF.WorkFlow.Api.Models.Entities
{
    /// <summary>
    /// entity model of wf_category
    /// </summary>
    [Table("wf_category")]
    public class WorkFlowCategoryEntity : GuidEntity
    {
        /// <summary>
        /// category
        /// </summary>
        public string Category { get; set; }
    }
}
