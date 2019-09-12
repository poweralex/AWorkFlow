using System.Collections.Generic;

namespace Mcs.SF.WorkFlow.Api.Models.Statuses
{
    /// <summary>
    /// data model for search condition
    /// </summary>
    public class SearchStatusRequest
    {
        /// <summary>
        /// targetIds
        /// </summary>
        public List<string> TargetIds { get; set; }
        /// <summary>
        /// statuses
        /// </summary>
        public List<string> Statuses { get; set; }
    }
}
