using System;
using System.Collections.Generic;
using System.Text;

namespace AWorkFlow.Core.Models
{
    public class JobExecutionResultDto
    {
        /// <summary>
        /// action execution results
        /// </summary>
        public List<ActionExecutionResultDto> ActionResults { get; set; } = new List<ActionExecutionResultDto>();
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
        /// <summary>
        /// execution message
        /// </summary>
        public string Message { get; set; }
    }
}
