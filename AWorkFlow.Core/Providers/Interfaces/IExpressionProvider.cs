using AWorkFlow.Core.Models;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Providers.Interfaces
{
    public interface IExpressionProvider
    {
        ArgumentsDto Arguments { get; }

        Task<ExpressionResultDto> Format(string expression);
    }
}
