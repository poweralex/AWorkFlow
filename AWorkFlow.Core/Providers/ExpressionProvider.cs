using AWorkFlow.Core.Models;
using AWorkFlow.Core.Providers.Interfaces;
using System;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Providers
{
    class ExpressionProvider : IExpressionProvider
    {
        public ArgumentsDto Arguments { get; private set; }

        public ExpressionProvider(ArgumentsDto arguments)
        {
            Arguments = arguments;
        }

        public Task<string> Format(string expression)
        {
            throw new NotImplementedException();
        }

        public Task<T> Format<T>(string expression)
        {
            throw new NotImplementedException();
        }
    }
}
