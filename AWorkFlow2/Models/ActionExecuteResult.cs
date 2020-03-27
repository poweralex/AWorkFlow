using System.Collections.Generic;

namespace AWorkFlow2.Models
{
    /// <summary>
    /// action execute result
    /// </summary>
    public class ActionExecuteResult
    {
        /// <summary>
        /// is success
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// is fail
        /// </summary>
        public bool Fail { get; set; }
        /// <summary>
        /// message
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// execute result
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// execute result data type
        /// </summary>
        public string DataType { get; set; }
        /// <summary>
        /// action output
        /// </summary>
        public Dictionary<string, string> Output { get; set; }
    }
}
