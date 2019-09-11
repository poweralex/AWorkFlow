using System;
using System.Collections.Generic;

namespace AWorkFlow.Core.Models
{
    public class WorkDto
    {
        public string WorkId { get; set; }
        public string WorkFlowCode { get; set; }
        public int WorkFlowVersion { get; set; }
        /// <summary>
        /// begin time
        /// </summary>
        public DateTime? BeginTime { get; set; }
        /// <summary>
        /// end time
        /// </summary>
        public DateTime? EndTime { get; set; }
        public string Status { get; set; }
        public List<WorkStatusDto> Statuses { get; set; }
        public List<WorkStepDto> WorkSteps { get; set; }
        public List<WorkDirectionDto> WorkDirections { get; set; }
    }

    public class WorkStatusDto
    {
        public string Status { get; set; }
        public DateTime? Time { get; set; }
    }

    public class WorkStepDto
    {
        public string WorkStepId { get; set; }
        public string StepCode { get; set; }
        public List<string> Tags { get; set; }
        public object TagData { get; set; }
        public string Group { get; set; }
        public int? MatchQty { get; set; }
        public ArgumentsDto Arguments { get; set; }
    }

    public class WorkDirectionDto
    {
        public string StepId { get; set; }
        public string NextStepId { get; set; }
    }

}
