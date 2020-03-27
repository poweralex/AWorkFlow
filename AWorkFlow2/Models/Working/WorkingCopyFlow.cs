using System.Collections.Generic;
using System.Linq;

namespace AWorkFlow2.Models.Working
{
    public class WorkingCopyFlow : WorkingModelBase
    {
        public WorkingCopyFlowSeed FromStep { get; set; }
        public WorkingCopyStepResult ExecutionResult { get; set; }
        public WorkingCopyFlowSeed ToStep { get; set; }
    }

    public class WorkingCopyFlowSeed
    {
        public WorkingCopyFlowSeed(WorkingCopyStep step)
        {
            StepGroup = new List<WorkingCopyStep> { step };
        }

        public WorkingCopyFlowSeed(IEnumerable<WorkingCopyStep> steps)
        {
            StepGroup = steps?.ToList();
        }
        public bool IsGroup { get { return StepGroup?.Count() > 1; } }
        public List<WorkingCopyStep> StepGroup { get; set; }

        /// <summary>
        /// determine if this seed contains the step
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public bool Contains(WorkingCopyStep step)
        {
            return StepGroup?.Contains(step) ?? false;
        }

        public IEnumerable<WorkingCopyStep> GetSteps()
        {
            return StepGroup;
        }

        public void AddStep(WorkingCopyStep step)
        {
            if (StepGroup == null)
            {
                StepGroup = new List<WorkingCopyStep>();
            }
            StepGroup.Add(step);
        }
    }
}
