using AWorkFlow.Core.Environments;
using AWorkFlow.Core.Models;
using AWorkFlow.Core.Providers.Interfaces;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Providers
{
    public class ExecutorProvider : IExecutorProvider
    {
        public Task<IExecutor> GetExecutor(ActionSettingDto settings)
        {
            return Task.FromResult(WorkFlowEnvironment.Instance.ResolveAction(settings.ActionType));
        }
    }
}
