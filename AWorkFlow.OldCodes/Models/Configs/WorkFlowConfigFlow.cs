using AutoMapper.Attributes;
using Mcs.SF.WorkFlow.Api.Models.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mcs.SF.WorkFlow.Api.Models.Configs
{
    /// <summary>
    /// workflow flow model
    /// </summary>
    [MapsTo(typeof(WorkFlowConfigFlowEntity), ReverseMap = true)]
    public class WorkFlowConfigFlow
    {
        /// <summary>
        /// current step code
        /// </summary>
        public string CurrentStepCode { get; set; }
        /// <summary>
        /// next step code
        /// </summary>
        public string NextStepCode { get; set; }
        /// <summary>
        /// next on condition
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public FlowNextType NextOn { get; set; }
    }

    /// <summary>
    /// NextOn types
    /// </summary>
    public enum FlowNextType
    {
        /// <summary>
        /// on current step success
        /// </summary>
        OnSuccess,
        /// <summary>
        /// on current step fail
        /// </summary>
        OnFail,
        /// <summary>
        /// on all steps of current group success
        /// </summary>
        OnGroupAllSuccess,
        /// <summary>
        /// on all steps of current group fail
        /// </summary>
        OnGroupAllFail,
        /// <summary>
        /// on any step of current group success
        /// </summary>
        OnGroupAnySuccess,
        /// <summary>
        /// on any step of current grou fail
        /// </summary>
        OnGroupAnyFail
    }
}
