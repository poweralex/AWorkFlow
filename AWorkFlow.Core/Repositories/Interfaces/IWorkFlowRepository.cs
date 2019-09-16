using AWorkFlow.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Repositories.Interfaces
{
    public interface IWorkFlowRepository
    {
        Task<bool> SaveWorkFlow(WorkFlowDto workFlow);
        Task<IEnumerable<WorkFlowDto>> SearchWorkFlow(string category, string code, int? version);
        Task<IEnumerable<WorkFlowDto>> GetWorkingFlow(string category);
    }
}
