using System;

namespace AWorkFlow.Core.Models
{
    /// <summary>
    /// execution result dto
    /// </summary>
    public class ExecutionResultDto
    {
        /// <summary>
        /// execute arguments
        /// </summary>
        public object ExecuteArguments { get; set; }
        /// <summary>
        /// execute result
        /// </summary>
        public object ExecuteResult { get; set; }
        /// <summary>
        /// execution time
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }
        /// <summary>
        /// if execution completed
        /// </summary>
        public bool Completed { get; set; }
        /// <summary>
        /// if execution succeeded
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// if execution failed
        /// </summary>
        public bool Fail { get; set; }
    }
}
