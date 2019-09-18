using AWorkFlow.Core.Environments;

namespace AWorkFlow.Core.Runner
{
    public class WorkFlowEngine
    {
        public static WorkFlowEnvironment Settings { get; set; }
        public static WorkManager Worker { get; set; }
        public static WorkFlowManager WorkFlow { get; set; }
    }
}
