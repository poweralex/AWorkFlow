using System;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Models
{
    public class JobDto
    {
        public JobType Type { get; set; }
        public WorkDto Work { get; set; }
        public WorkStepDto Step { get; set; }

        public Task<JobExecutionResultDto> Execute()
        {
            throw new NotImplementedException();
        }
    }

    public enum JobType
    {
        WorkPreAction,
        WorkAfterAction,
        StepPreAction,
        StepAction,
        StepAfterAction
    }
}
