using AWorkFlow2.Models.Configs;
using System.Collections.Generic;
using System.Linq;

namespace AWorkFlow2.Models.Working
{
    public class WorkingCopyGroup : WorkingModelBase
    {
        public string Id { get; set; }
        public string FLowId { get; set; }
        public string StartStepCode { get; set; }
        public string EndStepCode { get; set; }
        public bool Fulfilled { get; private set; }
        public bool AnySuccess => Steps?.Where(x => string.Equals(x.Code, EndStepCode, System.StringComparison.CurrentCultureIgnoreCase))?.Any(x => x.ActionFinished && x.Success) ?? false;
        public bool AnyFail => Steps?.Where(x => string.Equals(x.Code, EndStepCode, System.StringComparison.CurrentCultureIgnoreCase))?.Any(x => x.ActionFinished && !x.Success) ?? false;
        public bool AllSuccess => Fulfilled && (Steps?.Where(x => string.Equals(x.Code, EndStepCode, System.StringComparison.CurrentCultureIgnoreCase))?.Any(x => x.ActionFinished && x.Success) ?? false);
        public bool AllFail => Fulfilled && (Steps?.Where(x => string.Equals(x.Code, EndStepCode, System.StringComparison.CurrentCultureIgnoreCase))?.Any(x => x.ActionFinished && !x.Success) ?? false);
        public bool PostedNext { get; set; }
        public bool Finished => Fulfilled && PostedNext;
        public List<WorkingCopyStep> Steps { get; set; }

        public WorkingCopyGroup ReorganizeGroup(WorkingCopy work, WorkFlowConfig workflow)
        {
            var currentSteps = Steps.ToList();
            currentSteps.ForEach(startStep => Steps.AddRange(FindSteps(work, startStep, EndStepCode)));

            Fulfilled = Steps?.All(x => x.ActionFinished && (x.PostedNext || string.Equals(x.Code, EndStepCode, System.StringComparison.CurrentCultureIgnoreCase))) ?? false;
            return this;
        }

        public static IEnumerable<WorkingCopyGroup> BuildGroup(WorkingCopy work, WorkFlowConfig workflow, WorkFlowConfigFlow flow)
        {
            var startSteps = work.Steps.Where(x => x.Code == flow.GroupStartStepCode);
            var groups = startSteps.Select(startStep =>
            {
                var group = new WorkingCopyGroup
                {
                    FLowId = flow.Id,
                    StartStepCode = flow.GroupStartStepCode,
                    EndStepCode = flow.CurrentStepCode,
                    Steps = new List<WorkingCopyStep>
                    {
                        startStep
                    }
                };

                group.Steps.AddRange(FindSteps(work, startStep, flow.CurrentStepCode));

                return group;
            });

            return groups;
        }

        private static IEnumerable<WorkingCopyStep> FindSteps(WorkingCopy work, WorkingCopyStep startStep, string endStepCode)
        {
            List<WorkingCopyStep> steps = new List<WorkingCopyStep>();
            if (startStep.Code == endStepCode)
            {
                return steps;
            }
            var nextSteps = work.Flows.Where(x => x.FromStep.Contains(startStep)).SelectMany(x => x.ToStep.GetSteps()).ToList().Distinct();

            steps.AddRange(nextSteps);
            if (nextSteps?.Any() == true)
            {
                steps.AddRange(nextSteps.SelectMany(x => FindSteps(work, x, endStepCode)));
            }
            return steps;
        }
    }

}
