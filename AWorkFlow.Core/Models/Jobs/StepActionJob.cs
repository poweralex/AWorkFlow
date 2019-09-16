using AWorkFlow.Core.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Models.Jobs
{
    class StepActionJob : JobDto
    {
        [IgnoreDataMember]
        public WorkDto Work { get; set; }
        [IgnoreDataMember]
        public WorkStepDto Step { get; set; }

        public StepActionJob()
        {
            JobType = JobTypes.StepAction;
        }

        internal override async Task<IEnumerable<JobDto>> AfterSuccess(IJobRepository jobRepository, string user)
        {
            List<JobDto> nextJobs = new List<JobDto>();
            // post next step(s) by partial success
            var nextSteps = Work.WorkFlow.Flows.Where(x => x.StepCode == Step.StepCode && x.NextOn == WorkFlowNextOn.OnPartialSuccess);
            if (nextSteps?.Any() != true)
            {
                return nextJobs;
            }

            foreach (var direction in nextSteps)
            {
                nextJobs.AddRange(await Work.PostStep(jobRepository, Step, direction));
            }

            // update step result
            nextJobs.AddRange(await Step.UpdateStepResult(true, false));
            return nextJobs;
        }

        internal override Task<IEnumerable<JobDto>> AfterFail(IJobRepository jobRepository, string user)
        {
            // post next step(s) by partial fail
            // update step result
            // post next step(s) by fail
            // update group result
            // post next step by group all/any fail
            return base.AfterFail(jobRepository, user);
        }
    }
}
