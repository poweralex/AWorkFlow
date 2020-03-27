using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace AWorkFlow2.Models.Configs
{
    /// <summary>
    /// workflow flow model
    /// </summary>
    public class WorkFlowConfigFlow
    {
        /// <summary>
        /// flow id
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
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
        /// <summary>
        /// WorkFlow Selector
        /// </summary>
        public WorkFlowActionSetting Selector { get; set; }
        /// <summary>
        /// specific a group start while next on group-related condition
        /// </summary>
        public string GroupStartStepCode { get; set; }
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
        /// on execution success
        /// </summary>
        OnPartialSuccess,
        /// <summary>
        /// on execution fail
        /// </summary>
        OnPartialFail,
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
