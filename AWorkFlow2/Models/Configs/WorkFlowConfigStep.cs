using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AWorkFlow2.Models.Configs
{
    /// <summary>
    /// workflow step config
    /// </summary>
    public class WorkFlowConfigStep
    {
        /// <summary>
        /// step code
        /// </summary>
        [Required]
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
        public List<string> Tags { get; set; }
        /// <summary>
        /// loop by
        /// </summary>
        public string LoopBy { get; set; }
        /// <summary>
        /// is manual step
        /// </summary>
        public bool Manual { get; set; }
        /// <summary>
        /// if this step goes by qty
        /// </summary>
        public bool ByQty { get; set; }
        /// <summary>
        /// target qty to match if byQty
        /// </summary>
        public string MatchQty { get; set; }
        /// <summary>
        /// is begin step
        /// </summary>
        public bool IsBegin { get; set; }
        /// <summary>
        /// is end step
        /// </summary>
        public bool IsEnd { get; set; }
        /// <summary>
        /// retry count limit
        /// </summary>
        public int RetryLimit { get; set; }
        /// <summary>
        /// input variable names
        /// </summary>
        public List<string> Input { get; set; }
        /// <summary>
        /// step output(s)
        /// </summary>
        public Dictionary<string, string> Output { get; set; }
        /// <summary>
        /// time interval between 2 pre-action execution
        /// </summary>
        public TimeSpan? PreActionInterval { get; set; } = TimeSpan.FromMinutes(1);
        /// <summary>
        /// time interval between 2 action execution
        /// </summary>
        public TimeSpan? ActionInterval { get; set; } = TimeSpan.FromMinutes(1);
        /// <summary>
        /// pre-action, execute before actions
        /// </summary>
        public List<WorkFlowActionSetting> PreActions { get; set; }
        /// <summary>
        /// actions
        /// </summary>
        public List<WorkFlowActionSetting> Actions { get; set; }
    }
}
