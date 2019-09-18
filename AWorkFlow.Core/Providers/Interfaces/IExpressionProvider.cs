using AWorkFlow.Core.Models;

namespace AWorkFlow.Core.Providers.Interfaces
{
    public interface IExpressionProvider
    {
        ArgumentsDto Arguments { get; }

        ExpressionResultDto Format(string expression);
    }
}
