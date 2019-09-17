using AWorkFlow.Core.Models;
using AWorkFlow.Core.Providers.Interfaces;
using System.Threading.Tasks;

namespace AWorkFlow.VariableProcessAction.Processor
{
    interface IVariableProcessor
    {
        Task<ExecutionResultDto> Execute(VariableProcessActionSetting setting, IExpressionProvider expressionProvider);
    }
}
