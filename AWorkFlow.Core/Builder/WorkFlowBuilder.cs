using AWorkFlow.Core.Models;

namespace AWorkFlow.Core.Builder
{
    public class WorkFlowBuilder
    {
        private WorkFlowDto _workFlow;
        private WorkFlowBuilder()
        {

        }

        public static WorkFlowBuilder Create(string category, string code)
        {
            return new WorkFlowBuilder { _workFlow = new WorkFlowDto { Category = category, Code = code } };
        }

        public WorkFlowDto Build()
        {
            return _workFlow;
        }

        public WorkFlowBuilder First()
        {
            return this;
        }

        public WorkFlowBuilder Success()
        {
            return this;
        }

        public WorkFlowBuilder Fail()
        {
            return this;
        }
    }
}
