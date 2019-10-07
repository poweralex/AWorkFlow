using System;
using System.Collections.Generic;

namespace AWorkFlow.Core.Models
{
    public class JobResultDto
    {
        public JobDto Job { get; set; }
        /// <summary>
        /// submit time
        /// </summary>
        public DateTime? SubmitTime { get; set; }
        /// <summary>
        /// qty
        /// </summary>
        public int? Qty { get; set; }
        /// <summary>
        /// is success
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// is fail
        /// </summary>
        public bool Failed { get; set; }
        /// <summary>
        /// is cancelled
        /// </summary>
        public bool Cancelled { get; set; }
        /// <summary>
        /// execution result(s)
        /// </summary>
        public List<ActionExecutionResultDto> Executions { get; set; }
    }
}
