using System.Threading.Tasks;
using AWorkFlow.Core.Models;
using AWorkFlow.Core.Providers.Interfaces;

namespace AWorkFlow.Core.ActionExecutors
{
    public interface IActionExecutor
    {
        Task<ActionExecutionResultDto> Execute(IExpressionProvider expressionProvider, ActionSettingDto action);
    }
}
