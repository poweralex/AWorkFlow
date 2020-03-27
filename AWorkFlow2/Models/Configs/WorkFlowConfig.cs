using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AWorkFlow2.Models.Configs
{
    /// <summary>
    /// data model for workflow config
    /// </summary>
    public class WorkFlowConfig
    {
        /// <summary>
        /// Code
        /// </summary>
        [Required]
        public string Code { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Desc
        /// </summary>
        public string Desc { get; set; }
        /// <summary>
        /// Version
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// Category
        /// </summary>
        [Required]
        public string Category { get; set; }
        /// <summary>
        /// WorkFlow Selector
        /// </summary>
        public WorkFlowActionSetting Selector { get; set; }
        /// <summary>
        /// WorkFlow Steps
        /// </summary>
        [MinLength(1)]
        public List<WorkFlowConfigStep> Steps { get; set; }
        /// <summary>
        /// WorkFlow Flows
        /// </summary>
        public List<WorkFlowConfigFlow> Flows { get; set; }
        /// <summary>
        /// WorkFlow Output
        /// </summary>
        public Dictionary<string, string> Output { get; set; }
    }
}
