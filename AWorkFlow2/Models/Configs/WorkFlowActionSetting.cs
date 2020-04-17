using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace AWorkFlow2.Models.Configs
{
    /// <summary>
    /// data model for workflow action setting
    /// </summary>
    public class WorkFlowActionSetting
    {
        /// <summary>
        /// action type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ActionType Type { get; set; }
        /// <summary>
        /// execute sequence
        /// </summary>
        public int Sequence { get; set; }
        /// <summary>
        /// action config json
        /// </summary>
        public object ActionConfig { get; set; }
        /// <summary>
        /// action output
        /// </summary>
        public Dictionary<string, string> Output { get; set; }
    }

    /// <summary>
    /// action type
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// call web api action
        /// </summary>
        CallRestApi,
        /// <summary>
        /// variable process action
        /// </summary>
        VariableProcess
    }
}
