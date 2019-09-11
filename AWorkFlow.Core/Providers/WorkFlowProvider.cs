using AWorkFlow.Core.Models;
using AWorkFlow.Core.Providers.Interfaces;
using AWorkFlow.Core.Repositories.Interfaces;
using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
        }

        public Task<IEnumerable<WorkFlowDto>> SearchWorkFlow(string category)
        {
            throw new NotImplementedException();
        }

        public Task<WorkFlowDto> SetWorkFLow(WorkFlowDto workFlow)
        {
            throw new NotImplementedException();
        }
    }
}
