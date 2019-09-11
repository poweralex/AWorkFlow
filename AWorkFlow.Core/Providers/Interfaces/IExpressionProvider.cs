using AWorkFlow.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Providers.Interfaces
{
    public interface IExpressionProvider
    {
        ArgumentsDto Arguments { get; }

        Task<string> Format(string expression);
        Task<T> Format<T>(string expression);
    }
}
