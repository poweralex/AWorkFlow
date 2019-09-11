using AWorkFlow.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Providers.Interfaces
{
    public interface IWorkFlowProvider
    {
        Task<IEnumerable<WorkFlowDto>> GetWorkingFlows(string category);
        Task<WorkFlowDto> SetWorkFLow(WorkFlowDto workFlow);
        Task<IEnumerable<WorkFlowDto>> SearchWorkFlow(string category, string code, int? version);
        Task<WorkFlowDto> GetWorkFlow(string id);
    }
}
