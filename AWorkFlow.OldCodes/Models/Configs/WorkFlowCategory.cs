using AutoMapper.Attributes;
using Mcs.SF.WorkFlow.Api.Models.Entities;

namespace Mcs.SF.WorkFlow.Api.Models.Configs
{
    /// <summary>
    /// data model for workflow category
    /// </summary>
    [MapsTo(typeof(WorkFlowCategoryEntity), ReverseMap = true)]
    public class WorkFlowCategory
    {
        /// <summary>
        /// category
        /// </summary>
        public string Category { get; set; }
    }
}
