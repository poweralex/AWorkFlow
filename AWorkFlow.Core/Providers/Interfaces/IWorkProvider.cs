using AWorkFlow.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Providers.Interfaces
{
    public interface IWorkProvider
    {
        Task<IEnumerable<WorkDto>> Start(string category, object data, string user);
        Task<WorkDto> Stop(string id, string user);
        Task<WorkDto> GetWork(string id);
        Task<IEnumerable<WorkDto>> Search();
        Task<WorkDto> Hold(string id, string user);
        Task<WorkDto> Resume(string id, string user);
        Task<WorkDto> Restart(string id, string user);
        Task<WorkDto> Retry(string id, string user);

        Task PostStep(WorkStepDto workStep, string user);
    }
}
