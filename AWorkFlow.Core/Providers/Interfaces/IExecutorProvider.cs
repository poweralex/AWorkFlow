using AWorkFlow.Core.Models;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Providers.Interfaces
{
    public interface IExecutorProvider
    {
        Task<IExecutor> GetExecutor(ActionSettingDto settings);
    }
}
