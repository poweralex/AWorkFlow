using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AWorkFlow.Core.Models;

namespace AWorkFlow.Core.Runner
{
    /// <summary>
    /// provides workflow config related operations
    /// </summary>
    public class WorkFlowManager
    {
        public bool Add(WorkFlowDto workFlow)
        {
            throw new NotImplementedException();
        }

        public bool Disable(string code, int? version = null)
        {
            throw new NotImplementedException();
        }

        public Task<WorkFlowDto> GetWorkFlow(string code, int? version = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<WorkFlowDto>> GetWorkFlows(string category)
        {
            throw new NotImplementedException();
        }
    }
}
