using AWorkFlow.Core.Models;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Providers.Interfaces
{
    public interface IExecutor
    {
        Task<ExecutionResultDto> Execute(IExpressionProvider expressionProvider, ActionSettingDto action);
    }
}
