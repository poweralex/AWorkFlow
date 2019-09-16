using AWorkFlow.Core.Models;
using AWorkFlow.Core.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWorkFlow.InMemoryRepo
{
    public class WorkFlowInMemRepo : IWorkFlowRepository
    {
        static readonly object lockObj = new object();
        static readonly List<WorkFlowDto> _workFlows = new List<WorkFlowDto>();
        public Task<IEnumerable<WorkFlowDto>> GetWorkingFlow(string category)
        {
            List<WorkFlowDto> results = new List<WorkFlowDto>();
            var wfs = _workFlows.Where(x => string.Equals(category, x.Category, System.StringComparison.CurrentCultureIgnoreCase));
            var codes = wfs.Select(x => x.Code).Distinct(StringComparer.CurrentCultureIgnoreCase);
            foreach (var code in codes)
            {
                var maxVer = wfs.Where(x => x.Code == code).Max(x => x.Version);
                results.Add(wfs.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.CurrentCultureIgnoreCase) && x.Version == maxVer));
            }

            return Task.FromResult((IEnumerable<WorkFlowDto>)results);
        }

        public Task<bool> SaveWorkFlow(WorkFlowDto workFlow)
        {
            try
            {
                if (string.IsNullOrEmpty(workFlow?.Category)
                    || string.IsNullOrEmpty(workFlow?.Code))
                {
                    return Task.FromResult(false);
                }
                lock (lockObj)
                {
                    var sameCodeWfs = _workFlows.Where(x => x.Category == workFlow.Category && x.Code == workFlow.Code);
                    if (sameCodeWfs?.Any() == true)
                    {
                        workFlow.Version = sameCodeWfs.Max(x => x.Version) + 1;
                    }
                    else
                    {
                        workFlow.Version = 1;
                    }
                    _workFlows.Add(workFlow);
                }

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<IEnumerable<WorkFlowDto>> SearchWorkFlow(string category, string code, int? version)
        {
            return Task.FromResult(
                _workFlows.Where(x =>
                {
                    bool indicate = true;
                    if (indicate && !string.IsNullOrEmpty(category))
                    {
                        indicate &= string.Equals(x.Category, category, System.StringComparison.CurrentCultureIgnoreCase);
                    }
                    if (indicate && !string.IsNullOrEmpty(code))
                    {
                        indicate &= string.Equals(x.Code, code, System.StringComparison.CurrentCultureIgnoreCase);
                    }
                    if (indicate && version.HasValue)
                    {
                        indicate &= x.Version == version.Value;
                    }

                    return indicate;
                }));
        }
    }
}
