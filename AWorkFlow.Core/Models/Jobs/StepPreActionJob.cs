using AWorkFlow.Core.Providers;
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

        internal override async Task<IEnumerable<JobDto>> AfterSuccess()
        {
            // post step.action job
            var expressionProvider = new ExpressionProvider(new ArgumentsDto(PublicVariables));

            var nextJob = new StepActionJob
            {
                Id = Guid.NewGuid().ToString(),
                ActiveTime = DateTime.UtcNow,
                Actions = Step.WorkFlowStep.Actions,
                IsManual = Step.WorkFlowStep.IsManual,
                MatchQty = expressionProvider.Format(Step.WorkFlowStep.MatchQtyExp).GetResult<int?>(),
                PublicVariables = PublicVariables,
                Work = Work,
                WorkId = Work.WorkId,
                Step = Step,
                WorkStepId = Step.WorkStepId
            };
            return new List<JobDto> { nextJob };
        }
    }
}
