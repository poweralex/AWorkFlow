using AWorkFlow.Core.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Models.Jobs
{
    class StepPreActionJob : JobDto
    {
        [IgnoreDataMember]
        public WorkDto Work { get; set; }
        [IgnoreDataMember]
        public WorkStepDto Step { get; set; }

        public StepPreActionJob()
        {
            JobType = JobTypes.StepPreAction;
        }

        internal override async Task<IEnumerable<JobDto>> AfterSuccess(IJobRepository jobRepository, string user)
        {
            // post step.action job
            var nextJob = new StepActionJob
            {
                Id = Guid.NewGuid().ToString(),
                ActiveTime = DateTime.UtcNow,
                Actions = Step.WorkFlowStep.Actions,
                IsManual = Step.WorkFlowStep.IsManual,
                PublicVariables = PublicVariables,
                Work = Work,
                WorkId = Work.WorkId
            };
            await jobRepository.InsertJob(nextJob);
            return new List<JobDto> { nextJob };
        }
    }
}
