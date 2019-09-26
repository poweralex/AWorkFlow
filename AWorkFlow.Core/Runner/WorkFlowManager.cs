using AWorkFlow.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Runner
{
    /// <summary>
    /// provides workflow config related operations
    /// </summary>
    public class WorkFlowManager
    {
        public WorkFlowEngine Engine { get; set; }

        private readonly object _lockObj = new object();
        readonly List<WorkFlowDto> _workflows = new List<WorkFlowDto>();

        public async Task<bool> Add(WorkFlowDto workFlow)
        {
            try
            {
                lock (_lockObj)
                {
                    var flows = _workflows.Where(x => string.Equals(x.Code, workFlow?.Code, StringComparison.CurrentCultureIgnoreCase));
                    var version = 1;
                    if (flows.Any())
                    {
                        version = flows.Max(x => x.Version) + 1;
                    }
                    workFlow.Version = version;
                    _workflows.Add(workFlow);
                }
                return true;
            }
            catch
            {
            }
            return false;
        }

        public async Task<bool> Disable(string code, int? version = null)
        {
            try
            {
                lock (_lockObj)
                {
                    var flows = _workflows.Where(x => string.Equals(x.Code, code, StringComparison.CurrentCultureIgnoreCase));
                    if (version.HasValue)
                    {
                        flows = flows.Where(x => x.Version == version.Value);
                    }
                    if (flows.Any())
                    {
                        foreach (var flow in flows)
                        {
                            flow.Disabled = true;
                        }
                    }
                }
                return true;
            }
            catch
            {
            }
            return false;
            throw new NotImplementedException();
        }

        public async Task<WorkFlowDto> GetWorkFlow(string code, int? version = null)
        {
            var flows = _workflows.Where(x => string.Equals(x.Code, code, StringComparison.CurrentCultureIgnoreCase));
            if (!flows.Any())
            {
                return null;
            }
            int targetVersion = 0;

            if (version.HasValue)
            {
                targetVersion = version.Value;
            }
            else
            {
                targetVersion = flows.Where(x => !x.Disabled).Max(x => x.Version);
            }
            return flows.FirstOrDefault(x => x.Version == targetVersion);
        }

        public async Task<IEnumerable<WorkFlowDto>> GetWorkFlows(string category)
        {
            var flows = _workflows.Where(x => string.Equals(x.Category, category, StringComparison.CurrentCultureIgnoreCase));
            var flowGroup = flows.GroupBy(x => x.Code);
            return flowGroup.Select(x =>
            {
                var maxVer = x.Where(f => !f.Disabled).Max(f => f.Version);
                return x.FirstOrDefault(f => f.Version == maxVer);
            });
        }
    }
}
