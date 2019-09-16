using AWorkFlow.Core.Models;
using AWorkFlow.Core.Providers.Interfaces;
using AWorkFlow.Core.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Providers
{
    public class WorkFlowProvider : IWorkFlowProvider
    {
        private readonly IWorkFlowRepository _workFlowRepository;
        public WorkFlowProvider(IWorkFlowRepository workFlowRepository)
        {
            _workFlowRepository = workFlowRepository;
        }

        public Task<IEnumerable<WorkFlowDto>> GetWorkingFlows(string category)
        {
            return _workFlowRepository.GetWorkingFlow(category);
        }

        public Task<IEnumerable<WorkFlowDto>> SearchWorkFlow(string category, string code, int? version)
        {
            return _workFlowRepository.SearchWorkFlow(category, code, version);
        }

        public async Task<WorkFlowDto> SetWorkFLow(WorkFlowDto workFlow)
        {
            var res = await _workFlowRepository.SaveWorkFlow(workFlow);
            if (res)
            {
                return _workFlowRepository.SearchWorkFlow(workFlow.Category, workFlow.Code, null).Result.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }
    }
}
