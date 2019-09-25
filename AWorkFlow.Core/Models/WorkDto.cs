using System;
using System.Collections.Generic;

namespace AWorkFlow.Core.Models
{
    public class WorkDto
    {
        public string Id { get; set; }
        public WorkFlowDto WorkFlow { get; set; }
        public List<WorkStepDto> Steps { get; set; }

        public List<WorkStepDto> WorkingSteps { get; set; }
        public List<JobDto> RunningJobs { get; set; }
        public Dictionary<string, WorkGroupDto> Groups { get; set; }

        internal object Start()
        {
            throw new NotImplementedException();
        }
    }

    public class WorkStepDto
    {
    }

    public class WorkGroupDto
    { }
}
