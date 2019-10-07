namespace AWorkFlow.Core.Models
{
    public class JobDto
    {
        public string Type { get; set; }
        public WorkDto Work { get; set; }
        public WorkStepDto Step { get; set; }
    }

    public class JobType
    {
        public static readonly string WorkPreAction = "WorkPreAction";
        public static readonly string WorkAfterAction = "WorkAfterAction";
        public static readonly string StepPreAction = "StepPreAction";
        public static readonly string StepAction = "StepAction";
        public static readonly string StepAfterAction = "StepAfterAction";
    }
}
